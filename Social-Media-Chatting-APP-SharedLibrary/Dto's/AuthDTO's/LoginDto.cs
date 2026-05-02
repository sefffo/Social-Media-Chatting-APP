using System.ComponentModel.DataAnnotations;

namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s;

public class LoginDto
{
    [EmailAddress]
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;

}