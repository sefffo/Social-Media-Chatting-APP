using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Common;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s;
using Social_Media_Chatting_APP_SharedLibrary.Settings;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;
using StackExchange.Redis;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Social_Media_Chatting_APP_Service.Features.Authentication;

public class AuthService(
    IUnitOfWork unitOfWork,
    UserManager<AppUser> userManager,
    IOptions<AppSettings> appSettings,
    BackgroundEmailQueue emailQueue,
    IEmailService emailService,
    IOptions<JwtSettings> options,
    IOtpService otpService) : IAuthService
{
    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes); // OS-level true randomness
        return Convert.ToBase64String(bytes); // → safe 88-char Base64 string
    }

    private async Task<(string AccessToken, string jti)> GenerateJWTTokenAsync(AppUser user)
    {
        //get the roles 
        var roles = await userManager.GetRolesAsync(user);
        // create the jti (JWT ID) 64-char random string

        var jti = Guid.NewGuid().ToString();

        // Build the claims that will be embedded in the JWT payload.
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        //build a secrete key from the config (JwtSettings)

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //create the token and return it

        var jwt = new JwtSecurityToken(
            issuer: options.Value.Issuer,
            audience: options.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.Value.AccessTokenExpiryMinutes),
            signingCredentials: creds
        );
        // Serialize to compact form: Base64Url(header).Base64Url(payload).signature
        // This is the string the client puts in: Authorization: Bearer <token>
        return (new JwtSecurityTokenHandler().WriteToken(jwt), jti);
    }

    private static string ComputeSha256Hash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash); // e.g. "3A9FBC2D..."
    }


    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        // validate the Parameters 
        var parameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = false, // intentional as we ew gonna get the expired 
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SecretKey)),
            ValidIssuer = options.Value.Issuer,
            ValidAudience = options.Value.Audience,
            ValidateIssuerSigningKey = true,
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, parameters, out var validatedToken);

        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException(
                "Token uses an unexpected signing algorithm.");

        return principal;
    }

    public async Task<Result<LoginReturnDto>> LoginAsync(LoginDto dto)
    {
        //check that user exists first 
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return Result<LoginReturnDto>.Fail(Error.NotFound("Auth.UserNotFound", "User Not Found"));
        }

        //check the user password
        if (!await userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Error.InvalidCredentials("Auth.InvalidCredentials", "Invalid Credentials");
        }

        if (!user.EmailConfirmed)
        {
            await otpService.GenerateAndSendAsync(user.Id, OtpPurpose.EmailVerification);
            return Result<LoginReturnDto>.Fail(Error.BadRequest(
                "Auth.EmailNotVerified",
                "Please verify your email. A new code has been sent to your inbox."));
        }

        // ← NEW Gate: 2FA enabled?
        // WHY: User proved their password — but with 2FA on, that's only factor one.
        // We must NOT issue JWT here. Send OTP and tell frontend to show OTP screen.
        if (user.IsTwoFactorSetup)
        {
            await otpService.GenerateAndSendAsync(user.Id, OtpPurpose.TwoFactorLogin);
            return Result<LoginReturnDto>.Ok(new LoginReturnDto
            {
                RequiresTwoFactor = true,
                UserId = user.Id,
                AccessToken = null,
                RefreshToken = null
            });
        }

        // generate the AccessToken and Refresh Token 
        var (accessToken, jti) = await GenerateJWTTokenAsync(user);

        var RefreshToken = GenerateRefreshToken();

        var RefreshTokenHash = ComputeSha256Hash(RefreshToken);

        var repo = unitOfWork.GetRepository<RefreshToken, Guid>();

        await repo.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = RefreshTokenHash, // SHA-256 hash — safe to store
            JwtId = jti, // ← binds this row to the JWT above
            UserId = user.Id, // session owner
            ExpiresAt = DateTime.UtcNow.AddDays(7) // refresh window = 7 days
            // UsedAt   = null → not yet consumed
            // RevokedAt = null → not revoked
        });

        await unitOfWork.SaveChangesAsync();

        return Result<LoginReturnDto>.Ok(new LoginReturnDto()
        {
            AccessToken = accessToken,
            RefreshToken = RefreshToken
        });
    }

    public async Task<Result> RegisterAsync(RegisterDto dto)
    {
        // check for duplicate Email

        if (await userManager.FindByEmailAsync(dto.Email) != null)
        {
            return Result<object>.Fail(Error.BadRequest(
                "Auth.DuplicateEmail",
                $"An Account With this Email : {dto.Email} Already Exists "));
        }
        //if it's not duplicated, we go on with the registration Process   


        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            DisplayName = dto.DisplayName,
            CreatedAt = DateTime.UtcNow,
            IsGoogleAccount = false,
            IsOnline = false,
            IsTwoFactorSetup = false,
            EmailConfirmed = false
            // the bio and profile picture will be set later  depends on the user Service we gonna implement in phase 2
        };
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return Result<object>.Fail(
                result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList());
        }

        // ← Trigger OTP after successful registration
        // User can't login until they verify — VerifyOtpAsync sets EmailConfirmed=true
        await otpService.GenerateAndSendAsync(user.Id, OtpPurpose.EmailVerification);

        return Result<object>.Ok(new
        {
            UserId = user.Id,
            Message = "Registration successful. Please check your email for your verification code."
        });
    }

    public async Task<Result<LoginReturnDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        ClaimsPrincipal principal;
        try
        {
            // lets check the old Tokens if they are valid 
            principal = GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken);
        }
        catch
        {
            return Result<LoginReturnDto>.Fail(Error.Unauthorized("Auth.InvalidToken", "the access Token is Invalid"));
        }

        //get the userId and the jti from the old access Token principals 

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti); // get it from the registerClaims
        var incomingHash = ComputeSha256Hash(refreshTokenDto.RefreshToken);

        var repo = unitOfWork.GetRepository<RefreshToken, Guid>();
        //compare the stored in DB Token with the Incoming hashToken 
        var allValues = await repo.GetAllAsync();
        var stored = allValues.FirstOrDefault(r => r.TokenHash == incomingHash);
        // so we check now if every thing in the database are actually the same as we have from the request
        if (stored == null || // hash not found → token was never issued
            !stored.IsActive || // already used, revoked, or past ExpiresAt
            stored.JwtId != jti || // ← jti mismatch: tokens not from same pair
            stored.UserId != userId) // token belongs to a DIFFERENT user
        {
            return Result<LoginReturnDto>.Fail(Error.Unauthorized("Auth.InvalidToken", "the access Token is Invalid"));
        }

        // end the old Token authorization 
        stored.UsedAt = DateTime.UtcNow;
        stored.RevokedAt = DateTime.UtcNow;

        //now generate the new Token 
        var user = await userManager.FindByIdAsync(userId);
        //get the new access token with its jti 
        var (newAccessToken, newJti) = await GenerateJWTTokenAsync(user);

        var newRawRefreshToken = GenerateRefreshToken();

        var newRefreshTokenHashed = ComputeSha256Hash(newRawRefreshToken);

        await repo.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = newRefreshTokenHashed,
            JwtId = newJti, // new JWT's jti
            UserId = user!.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await unitOfWork.SaveChangesAsync();

        return Result<LoginReturnDto>.Ok(new LoginReturnDto()
        {
            AccessToken = newAccessToken,
            RefreshToken = newRawRefreshToken,
        });
    }

    /// <summary>
    /// Terminates a session by permanently revoking a refresh token.
    ///
    /// Call this on logout. After this, the token's row has RevokedAt set,
    /// which makes IsActive = false, blocking any future refresh attempts.
    ///
    /// For full logout across ALL devices, call this for every active
    /// refresh token row for the user (filter by UserId, RevokedAt == null).
    /// </summary>
    public async Task<Result> RevokeRefreshTokenAsync(string rawRefreshToken)
    {
        var hash = ComputeSha256Hash(rawRefreshToken);
        var repo = unitOfWork.GetRepository<RefreshToken, Guid>();

        // Load all and find by hash — same pattern as RefreshTokenAsync
        // to avoid ParallelEnumerable type-inference issues.
        var all = await repo.GetAllAsync();
        var stored = all.FirstOrDefault(x => x.TokenHash == hash);

        if (stored is null || !stored.IsActive)
            return Result<object>.Fail(
                Error.NotFound(
                    "Auth.TokenNotFound",
                    "Refresh token not found or already revoked."));

        // Stamp RevokedAt — IsActive becomes false immediately.
        // The row is kept for audit purposes, not deleted.
        stored.RevokedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync();

        return Result<object>.Ok("Token Revoked");
    }

    /// <summary>
    /// first step in the reset Password process
    /// 1=>get the user if found ==> Return OK for the Data integrity and safety as he doesn't know
    /// 2=>if yes exist check if he still has valid Reset Tokens if yes ?  kill the old ones generate a new one
    ///    Old links sitting in their inbox become dead.
    /// 3=>Generate a new safe Token and add it to the DB
    /// 4=>Build a rest Link
    /// 5=> add the Link to the Email service Queue (Background job) 
    /// 6=> return OK as Email Sent 
    /// If this email is registered, a reset link has been sent
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<Result> ForgotPasswordAsync(string email)
    {
        //find the user by email first 
        var user = await userManager.FindByEmailAsync(email);
        //check on his state if found or not 

        if (user is null)
        {
            return Result<object>.Ok("If this email is registered, a reset link has been sent.");
        }

        //get the repo 
        var repo = unitOfWork.GetRepository<PasswordResetToken, Guid>();
        //validate all the active tokens he has bec => only the Last Token must be alive 

        //check on 3 things 
        //1=> if user already in the DB 
        //2=> if he has Valid Token and 3=>Active 
        var allExistResetTokens = (await repo.GetAllAsync()).Where(t => t.UserId == user.Id &&
                                                                        t.IsUsed == false &&
                                                                        t.ExpiresAt > DateTime.UtcNow).ToList();

        foreach (var old in allExistResetTokens)
        {
            old.IsUsed = true; //to kill any unused Tokens 
        }


        // ④ Generate a cryptographically secure URL-safe token
        // WHY NOT Guid: GUIDs are not cryptographically random — use OS-level randomness
        // 64 bytes → 512 bits of entropy → practically impossible to brute force
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-") // + and / are not URL-safe
            .Replace("/", "_") // replace with URL-safe equivalents
            .Replace("=", ""); // remove padding, not needed for our purposes

        //build a save Token 

        var resetToken = new PasswordResetToken()
        {
            Token = token,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15), //only live for 15 min 
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
        };
        //add the Token To the DB 
        await repo.AddAsync(resetToken);
        await unitOfWork.SaveChangesAsync();
        //generate the Link 
        //get the base URL Based on the Env 
        var BaseUrl = appSettings.Value.BaseUrl;
        var resetLink = $"{BaseUrl}/reset-password?token={token}";
        //add the Email Sending Link to the Background job 

        await emailQueue.EnqueueAsync(async (ct) =>
            await emailService.SendAsync(
                email,
                "Reset Your ConnectO Account Password",
                $"""
                 <div style="font-family:Arial,sans-serif;max-width:500px;margin:auto">
                     <h2 style="color:#4F46E5">Password Reset Request</h2>
                     <p>Hi {user.DisplayName ?? user.UserName},</p>
                     <p>We received a request to reset your ConnectO password.</p>
                     <p>Click the button below — this link expires in <strong>15 minutes</strong>.</p>
                     <a href="{resetLink}" 
                        style="display:inline-block;padding:12px 24px;background:#4F46E5;
                               color:white;border-radius:6px;text-decoration:none;font-weight:bold">
                         Reset Password
                     </a>
                     <p style="color:#888;font-size:12px;margin-top:20px">
                         If you didn't request this, you can safely ignore this email.
                     </p>
                 </div>
                 """
            ));


        return Result<object>.Ok("If this email is registered, a reset link has been sent.");
    }

    /// <summary>
    /// 1=>look for the Token in The DB
    /// 2=> three gate validation Token (exist => valid => Not Used or Expired)
    /// 3=> Find the User With this Token in the DB
    /// 4=> Reset the password
    /// 5=> kill that Token in His Email  
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var repo = unitOfWork.GetRepository<PasswordResetToken, Guid>();
        var all = await repo.GetAllAsync();
        var resetToken = all.FirstOrDefault(t => t.Token == dto.Token);
        //three gate validation 

        if (resetToken is null)
        {
            return Result<object>.Fail(Error.NotFound("Auth.InvalidToken", "This reset link is invalid."));
        }

        if (resetToken.IsUsed)
        {
            return Result<object>.Fail(Error.NotFound("Auth.TokenUsed", "This reset link has already been used."));
        }

        //expired 
        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return Result<object>.Fail(Error.NotFound("Auth.TokenExpired",
                "This reset link has expired. Please request a new one."));
        }

        var user = await userManager.FindByIdAsync(resetToken.UserId);
        //check if user exist 
        if (user is null)
        {
            return Result<object>.Fail(Error.NotFound(
                "Auth.UserNotFound", "User not found."));
        }

        // ④ Reset the password using ASP.NET Identity
        // WHY RemovePasswordAsync + AddPasswordAsync instead of direct hash?
        // Identity handles all the hashing, salting, and validation rules for us
        var removeResult = await userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return Result<object>.Fail(Error.BadRequest(
                "Auth.PasswordResetFailed", "Failed to reset password."));

        var addResult = await userManager.AddPasswordAsync(user, dto.NewPassword);
        if (!addResult.Succeeded)
            return Result<object>.Fail(
                addResult.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList());

        //kill the Token used 
        //check ot at the DB 
        resetToken.IsUsed = true;
        await unitOfWork.SaveChangesAsync();
        return Result<object>.Ok("Reset Password Success");
    }

    public async Task<Result> EnableTwoFactorAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<object>.Fail(Error.NotFound("Auth.UserNotFound", "User not found."));

        // Idempotency check — enabling already-enabled 2FA is a no-op
        // WHY: Don't throw an error, just inform. Idempotent operations are
        // safer — if the client calls this twice it won't break anything.
        if (user.IsTwoFactorSetup)
            return Result<object>.Fail(Error.BadRequest(
                "Auth.2FAAlreadyEnabled", "Two-factor authentication is already enabled."));

        //enable the 2FA
        user.IsTwoFactorSetup = true;

        //update the user 
        await userManager.UpdateAsync(user);

        // Notify the user — if someone else enabled 2FA on their account
        // without their knowledge, this email is their alert
        await emailQueue.EnqueueAsync(async (ct) =>
            await emailService.SendAsync(
                user.Email!,
                "Two-Factor Authentication Enabled — ConnectO",
                $"""
                 <div style="font-family:Arial,sans-serif;max-width:500px;margin:auto">
                     <h2 style="color:#4F46E5">2FA Enabled</h2>
                     <p>Hi {user.DisplayName ?? user.UserName},</p>
                     <p>Two-factor authentication has been <strong>enabled</strong> on your ConnectO account.</p>
                     <p>Every login will now require a verification code sent to this email.</p>
                     <p style="color:#888;font-size:12px">
                         If you didn't do this, please reset your password immediately.
                     </p>
                 </div>
                 """
            )
        );
        return Result<object>.Ok("Two-factor authentication enabled successfully.");
    }

    public async Task<Result> DisableTwoFactorAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<object>.Fail(Error.NotFound("Auth.UserNotFound", "User not found."));

        if (!user.IsTwoFactorSetup)
            return Result<object>.Fail(Error.BadRequest(
                "Auth.2FANotEnabled", "Two-factor authentication is not enabled."));

        user.IsTwoFactorSetup = false;
        await userManager.UpdateAsync(user);

        return Result<object>.Ok("Two-factor authentication disabled successfully.");
    }


    /// <summary>
    /// Orchestrates OTP verification + JWT issuance.
    /// Step 1: Delegate code validation to OtpService (Redis logic lives there)
    /// Step 2: If valid, confirm email + issue full JWT pair
    /// AuthService never touches Redis — it only asks OtpService "is this code valid?"
    /// </summary>
    public async Task<Result<LoginReturnDto>> VerifyOtpAsync(VerifyOtpDto dto)
    {
        var user = await userManager.FindByIdAsync(dto.UserId);
        if (user is null)
            return Result<LoginReturnDto>.Fail(
                Error.NotFound("Auth.UserNotFound", "User not found."));

        // ② Determine purpose from user state
        // If email not confirmed yet → this is EmailVerification flow
        // If email confirmed + 2FA setup → this is TwoFactorLogin flow
        var purpose = !user.EmailConfirmed
            ? OtpPurpose.EmailVerification
            : OtpPurpose.TwoFactorLogin;

        // ① Ask OtpService to validate the code — all Redis logic is inside there
        var verifyResult = await otpService.VerifyAsync(
            dto.UserId, dto.Code, purpose);


        // ② If OtpService says no → bubble the error up, never issue a token
        if (!verifyResult.IsSuccess)
            return Result<LoginReturnDto>.Fail(verifyResult.Errors.ToList());

        // ④ Code verified — NOW safe to confirm email if this was EmailVerification
        // WHY here and not before: we only confirm after proving the code is correct
        if (purpose == OtpPurpose.EmailVerification)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        // ⑤ Issue the full JWT pair — same pattern as LoginAsync
        var (accessToken, jti) = await GenerateJWTTokenAsync(user);
        var rawRefreshToken = GenerateRefreshToken();
        var hashedRefreshToken = ComputeSha256Hash(rawRefreshToken);

        var repo = unitOfWork.GetRepository<RefreshToken, Guid>();
        await repo.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = hashedRefreshToken,
            JwtId = jti,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await unitOfWork.SaveChangesAsync();

        return Result<LoginReturnDto>.Ok(new LoginReturnDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken
        });
    }

    public async Task<Result<LoginReturnDto>> HandleGoogleLoginAsync(string email, string name, string googleId)
    {
        // 1. Primary lookup by Google identity — correct first check
        var user = await userManager.FindByLoginAsync("Google", googleId);

        if (user is null)
        {
            // 2. Check if email exists as a normal account
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser is not null && !existingUser.IsGoogleAccount)
                return Result<LoginReturnDto>.Fail(Error.BadRequest(
                    "Auth.EmailConflict",
                    "This email is already registered. Please login with your password."));

            // 3. Create new Google user
            user = new AppUser
            {
                UserName = email, // ← email not name, must be unique
                Email = email,
                DisplayName = name,
                IsGoogleAccount = true,
                IsOnline = false,
                IsTwoFactorSetup = false,
                CreatedAt = DateTime.UtcNow
            };

            var randomPassword = Guid.NewGuid().ToString() + "!A1";
            var createResult = await userManager.CreateAsync(user, randomPassword);

            if (!createResult.Succeeded)
                return Result<LoginReturnDto>.Fail(
                    createResult.Errors
                        .Select(e => Error.Validation(e.Code, e.Description))
                        .ToList());

            // 4. Link Google identity — no re-fetch needed, user.Id already populated
            await userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleId, "Google"));
        }

        // 5. Generate token pair
        var rawRefreshToken = GenerateRefreshToken();
        var hashedRefreshToken = ComputeSha256Hash(rawRefreshToken);
        var (accessToken, jti) = await GenerateJWTTokenAsync(user);

        // 6. Persist refresh token
        var repo = unitOfWork.GetRepository<RefreshToken, Guid>();
        await repo.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = hashedRefreshToken,
            JwtId = jti,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await unitOfWork.SaveChangesAsync();

        return Result<LoginReturnDto>.Ok(new LoginReturnDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken
        });
    }
}