using FluentValidation;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s;

namespace Social_Media_Chatting_APP_Service.Features.Authentication.Validators;

/// <summary>
/// Validates ResetPasswordDto before it reaches AuthService.
/// Token format is intentionally not validated beyond NotEmpty —
/// we don't want to reveal our token format to potential attackers.
/// The service layer handles token existence and expiry checks.
/// </summary>
public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}
