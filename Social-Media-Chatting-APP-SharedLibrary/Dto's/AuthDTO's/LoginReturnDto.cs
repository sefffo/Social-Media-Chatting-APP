namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s;

public class LoginReturnDto
{
    public string? AccessToken { get; set; } 
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// True when the user has 2FA enabled.
    /// When true, AccessToken and RefreshToken are null —
    /// the client must redirect to the OTP screen and call
    /// POST /auth/verify-otp to get the actual tokens.
    /// UserId is populated so the client knows who to verify.
    /// </summary>
    public bool RequiresTwoFactor { get; set; }

    public string? UserId { get; set; }
    
    
}