using Social_Media_Chatting_APP_SharedLibrary.Dto_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_ServiceAbstraction;
/// <summary>
/// Contract for all authentication operations.
///
/// DESIGN NOTES:
/// ─────────────
/// • GenerateJWTTokenAsync is intentionally NOT here.
///   Token generation is an internal implementation detail of
///   AuthenticationService (private method). Exposing it on the
///   interface would break encapsulation — callers should never
///   generate tokens directly; they go through LoginAsync.
///
/// • RefreshTokenAsync returns Result => (LoginReturnDto) (not RefreshTokenDto)
///   because the refresh operation issues a completely new token pair
///   (new access token + new refresh token), which is identical in
///   shape to what LoginAsync returns.
///
/// • RevokeRefreshTokenAsync is the logout operation. Calling it
///   marks the refresh token as revoked in the DB so it can never
///   be used again, effectively terminating the session.
/// </summary>
public interface IAuthService
{
    
    Task<Result<LoginReturnDto>> LoginAsync(LoginDto dto);
    
    
    Task<Result> RegisterAsync(RegisterDto dto);
    
    /// <summary>
    /// Accepts an expired access token + a raw refresh token.
    /// Validates the pair, rotates the refresh token (old one is revoked,
    /// new one is issued), and returns a fresh token pair.
    /// Returns Unauthorized if the token is invalid, expired, already used,
    /// or revoked.
    /// </summary>
    Task<Result<LoginReturnDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);

    /// <summary>
    /// Revokes a refresh token by its raw value (logout). ==> the one that gonna be decoded in the code that comes hashed from the DB 
    /// After calling this, the token cannot be used to get a new access token.
    /// Call this on logout or when a security event is detected.
    /// </summary>
    Task<Result> RevokeRefreshTokenAsync(string rawRefreshToken);
    
    
    
    Task<Result> ForgotPasswordAsync(string email);
    
    
    Task<Result> ResetPasswordAsync(ResetPasswordDto dto);
    
    
    Task<Result<LoginReturnDto>> VerifyOtpAsync(VerifyOtpDto dto);

    
    Task<Result> ResendOtpAsync(string userId);
    
    /// <summary>
    /// handling google Authentication 
    /// </summary>
    /// <param name="email"></param>
    /// <param name="name"></param>
    /// <param name="googleId"></param>
    /// <returns></returns>
    Task<Result<LoginDto>> HandleGoogleLoginAsync(string email, string name, string googleId);


}