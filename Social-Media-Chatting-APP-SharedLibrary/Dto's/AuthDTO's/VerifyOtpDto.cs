using System.ComponentModel.DataAnnotations;

namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s;

public class VerifyOtpDto
{
    [Required]
    public string UserId { get; set; }

    [Required]
    [Length(6, 6)]          // OTP is exactly 6 digits, no more no less
    public string Code { get; set; }
}