using FluentValidation;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s;

namespace Social_Media_Chatting_APP_Service.Features.Authentication.Validators;

/// <summary>
/// Validates RegisterDto before it reaches AuthService.
/// Runs automatically via ValidationBehavior in the MediatR pipeline.
///
/// WHY duplicate some rules that are already on the DTO via DataAnnotations?
/// DataAnnotations fire at the model-binding layer (MVC).
/// FluentValidation fires inside our MediatR pipeline — it gives us
/// richer rules (Regex, cross-field), consistent error shapes, and
/// testability without spinning up the full HTTP stack.
/// </summary>
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(2).WithMessage("Username must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MinimumLength(3).WithMessage("Display name must be at least 3 characters.")
            .MaximumLength(30).WithMessage("Display name cannot exceed 30 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        // Cross-field rule: ConfirmPassword must match Password
        // WHY here and not just DataAnnotations [Compare]?
        // [Compare] fires at MVC binding — FluentValidation fires in our pipeline.
        // Having it here means it's caught at the same layer as all other rules
        // and returns the same error envelope shape.
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}
