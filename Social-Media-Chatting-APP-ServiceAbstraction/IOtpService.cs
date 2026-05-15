using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_ServiceAbstraction;

/// <summary>
/// Handles all OTP lifecycle operations: generation, delivery, and verification.
/// Uses Redis for storage — codes auto-expire via TTL, zero cleanup needed.
/// Designed to be reused across multiple flows (registration, 2FA, password reset).
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a 6-digit OTP, stores it in Redis with a 10-minute TTL,
    /// and enqueues an email to the user. Any previous OTP for the same
    /// (userId, purpose) is overwritten automatically.
    /// </summary>
    Task<Result> GenerateAndSendAsync(string userId, OtpPurpose purpose);

    /// <summary>
    /// Validates the submitted code against what's stored in Redis.
    /// Tracks failed attempts — invalidates the OTP after 3 wrong tries.
    /// Returns Ok on success, Fail with reason on any validation failure.
    /// </summary>
    Task<Result> VerifyAsync(string userId, string code, OtpPurpose purpose);
    
    /// <summary>
    /// Generates and sends a fresh OTP, overwriting any existing one.
    /// Delegates entirely to GenerateAndSendAsync — exists as a
    /// named method for clarity at the call site.
    /// </summary>
    Task<Result> ResendOtpAsync(string userId, OtpPurpose purpose);

}