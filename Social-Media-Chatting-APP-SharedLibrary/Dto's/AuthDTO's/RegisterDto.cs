using System.ComponentModel.DataAnnotations;

namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { set; get;}
    [Required]
    [MinLength(8)]
    public string Password { set; get; }
    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { set; get; }
    [Required]
    [MinLength(2), MaxLength(50)]
    public string UserName  { set; get; }
    [Required]
    [MinLength(3), MaxLength(30)]
    public string DisplayName  { set; get; }
}