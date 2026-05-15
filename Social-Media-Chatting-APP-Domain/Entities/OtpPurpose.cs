namespace Social_Media_Chatting_APP_Domain.Entities;

public enum OtpPurpose : byte
{
    /// <summary>Sent after registration to verify the user owns the email.</summary>
    EmailVerification=1,

    /// <summary>Sent as the second factor during login when 2FA is enabled.</summary>
    TwoFactorLogin=2,

    /// <summary>Sent as an alternative to the reset link for password recovery.</summary>
    PasswordReset=3
    
}