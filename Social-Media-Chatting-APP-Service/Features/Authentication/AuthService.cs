using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
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
    ILogger<AuthService> logger ,
    IHttpContextAccessor accessor,
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<JwtSettings> options ) : IAuthService
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
            ValidIssuer   = options.Value.Issuer,
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
            // the bio and profile picture will be set later  depends on the user Service we gonna implement in phase 2
        };
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return Result<object>.Fail(
                result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList());
        }

        return Result<Object>.Ok(result);
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
            RefreshToken =newRawRefreshToken ,
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

    public Task<Result> ForgotPasswordAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<Result<LoginReturnDto>> VerifyOtpAsync(VerifyOtpDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<Result> ResendOtpAsync(string userId)
    {
        throw new NotImplementedException();
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