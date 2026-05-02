using System.ComponentModel.DataAnnotations;

namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s;

public class ResetPasswordDto
{
    [Required]
    public string Token { get; set; }        // the Guid from the email link

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; }

    [Required]
    [Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; }
}