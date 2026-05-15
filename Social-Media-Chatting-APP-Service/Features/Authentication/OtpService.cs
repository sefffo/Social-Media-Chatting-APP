using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Service.Common;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;
using StackExchange.Redis;

namespace Social_Media_Chatting_APP_Service.Features.Authentication;

/// <summary>
/// Redis-backed OTP service.
/// Each OTP is stored under: "otp:{userId}:{purpose}"
/// Value is JSON-serialized OTPDataDto: { Code, UserId, Attempts }
/// TTL = 10 minutes → Redis auto-deletes expired codes, zero cleanup needed.
/// </summary>
public class OtpService(
    IConnectionMultiplexer redisConnection,
    UserManager<AppUser> userManager,
    IEmailService emailService,
    BackgroundEmailQueue emailQueue) : IOtpService
{
    // Every OTP lives exactly 10 minutes — then Redis kills it automatically
    private static readonly TimeSpan OtpExpiry = TimeSpan.FromMinutes(10);

    // After 3 wrong guesses the code is invalidated — prevents brute force
    private const int MaxAttempts = 3;

    /// <summary>
    /// Builds the namespaced Redis key for a (userId, purpose) pair.
    /// WHY namespace: A user could have a login OTP and a password reset OTP
    /// active at the same time — different keys prevent them from colliding.
    /// e.g. "otp:abc123:EmailVerification"
    /// </summary>
    private static string BuildKey(string userId, OtpPurpose purpose)
        => $"otp:{userId}:{purpose}";

    public async Task<Result> GenerateAndSendAsync(string userId, OtpPurpose purpose)
    {
        // Find the user — need their email and display name for the email

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result<object>.Fail(Error.NotFound("Otp.UserNotFound", "User Not Found"));
        }
        // ② Generate a cryptographically secure 6-digit code
        // WHY NOT Random.Next(): System.Random is seeded from time — predictable.
        // RandomNumberGenerator uses OS-level entropy — truly unpredictable.
        // We get 4 random bytes → convert to int → mod 1,000,000 → pad to 6 digits
        // e.g. 4821 becomes "004821" — always exactly 6 chars

        var randomBytes = new byte[4];
        RandomNumberGenerator.Fill(randomBytes);
        var randomInt = Math.Abs(BitConverter.ToInt32(randomBytes, 0));
        var code = (randomInt % 1_000_000).ToString("D6");
        // ③ Build the data object to store in Redis
        var otpData = new OTPDataDto(Code: code, UserId: userId, Attempts: 0);

        var db = redisConnection.GetDatabase();
        var key = BuildKey(user.Id, purpose);
        await db.StringSetAsync(key, JsonSerializer.Serialize(otpData), OtpExpiry);
        // ⑤ Enqueue the email — HTTP response returns immediately after this line
        // The background worker picks it up and sends asynchronously
        await emailQueue.EnqueueAsync(async (ct) =>
            await emailService.SendAsync(
                user.Email!,
                "Your ConnectO Verification Code",
                $"""
                 <div style="font-family:Arial,sans-serif;max-width:500px;margin:auto">
                     <h2 style="color:#4F46E5">Your Verification Code</h2>
                     <p>Hi {user.DisplayName ?? user.UserName},</p>
                     <p>Use the code below to verify your identity on ConnectO.</p>
                     <div style="font-size:40px;font-weight:bold;letter-spacing:10px;
                                 color:#4F46E5;text-align:center;padding:24px 0;
                                 background:#F5F3FF;border-radius:8px;margin:16px 0">
                         {code}
                     </div>
                     <p>This code expires in <strong>10 minutes</strong> and can only be used once.</p>
                     <p style="color:#888;font-size:12px;margin-top:20px">
                         If you didn't request this, you can safely ignore this email.
                     </p>
                 </div>
                 """
            )
        );

        return Result<object>.Ok("Verification code sent successfully.");
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="code"></param>
    /// <param name="purpose"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<Result> VerifyAsync(string userId, string code, OtpPurpose purpose)
    {
        var db=redisConnection.GetDatabase();
        var key = BuildKey(userId, purpose);
        // ① Try to read the OTP from Redis
        var raw =  await db.StringGetAsync(key);
        
        // Gate 1: Key exists in Redis?
        // RedisValue.HasValue is false if the key doesn't exist OR already expired by TTL
        // Both cases mean "no active OTP" — same error message, no info leakage
        if (!raw.HasValue)
            return Result<object>.Fail(Error.BadRequest(
                "Otp.NotFound", "No active code found. Please request a new one."));

        // ② Deserialize the stored data
        var otpData =  JsonSerializer.Deserialize<OTPDataDto>(raw.ToString()!);
        // Gate 2: Brute force check — too many wrong attempts?
        // WHY: Without this, attacker could try all 1,000,000 possible codes.
        // After 3 wrong tries the code is dead — must request a fresh one.
        if (otpData!.Attempts >= MaxAttempts)
        {
            await db.KeyDeleteAsync(key); // clean up immediately
            return Result<object>.Fail(Error.BadRequest(
                "Otp.MaxAttemptsReached",
                "Too many incorrect attempts. Please request a new code."));
        }
        // Gate 3: Does the submitted code match?
        if (otpData.Code != code)
        {
            // Increment attempts and write back — preserve the remaining TTL
            // WHY preserve TTL: we don't want wrong attempts to reset the expiry window
            var remainingTtl = await db.KeyTimeToLiveAsync(key);
            var updated = otpData with { Attempts = otpData.Attempts + 1 };
            await db.StringSetAsync(key, JsonSerializer.Serialize(updated), new Expiration(remainingTtl ?? OtpExpiry));
            var attemptsLeft = MaxAttempts - updated.Attempts;
            return Result<object>.Fail(Error.BadRequest(
                "Otp.InvalidCode",
                $"Invalid code. {attemptsLeft} attempt(s) remaining."));
            
        }
        // ③ All gates passed — delete the key immediately
        // WHY explicit delete and not just let TTL expire it?
        // A verified code must be dead instantly — TTL could leave a 9-minute window
        // where a valid verified code could theoretically be replayed
        await db.KeyDeleteAsync(key);

        return Result<object>.Ok("Code verified successfully.");
    }

    public async Task<Result> ResendOtpAsync(string userId,OtpPurpose purpose)
    {
        // Entirely delegates to GenerateAndSendAsync
        // The new code overwrites the old one in Redis automatically
        // The TTL resets to a fresh 10 minutes as well
        return await GenerateAndSendAsync(userId, purpose);    
    }
}