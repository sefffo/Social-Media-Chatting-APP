namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s;

/// <summary>
/// Request body for POST /api/auth/resend-otp
/// Only needs userId — the purpose is inferred from the user's state
/// same as VerifyOtpAsync: EmailConfirmed=false → EmailVerification, else → TwoFactorLogin
/// </summary>

public record ResendOtpDto(string UserId);