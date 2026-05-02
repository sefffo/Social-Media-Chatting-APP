using System.Security.Cryptography;
using System.Text;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Authentication;

public class AuthService : IAuthService
{
    
    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);        // OS-level true randomness
        return Convert.ToBase64String(bytes);     // → safe 88-char Base64 string
    }

    private async Task<(string AccessToken, string jti)> GenerateJWTTokenAsync(AppUser user)
    {
        throw new NotImplementedException();
    }
    
    private static string ComputeSha256Hash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash); // e.g. "3A9FBC2D..."
    }

    
    public Task<Result<LoginReturnDto>> LoginAsync(LoginDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<Result> RegisterAsync(RegisterDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<Result<LoginReturnDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        throw new NotImplementedException();
    }

    public Task<Result> RevokeRefreshTokenAsync(string rawRefreshToken)
    {
        throw new NotImplementedException();
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

    public Task<Result<LoginDto>> HandleGoogleLoginAsync(string email, string name, string googleId)
    {
        throw new NotImplementedException();
    }
}