using FluentValidation;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s;

namespace Social_Media_Chatting_APP_Service.Features.Authentication.Validators;

/// <summary>
/// Validates LoginDto before it reaches AuthService.
/// Kept intentionally minimal — we do NOT check password complexity here.
/// WHY: Telling an attacker "invalid email format" on a login attempt
/// leaks less info than telling them their password is too short.
/// We just confirm both fields are present and the email looks like an email.
/// </summary>
public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
