using FluentValidation;

namespace Social_Media_Chatting_APP_Service.Features.UserProfile.Commands.UpdateProfile;

/// <summary>
/// Validates UpdateProfileCommand before it reaches the handler.
/// All fields are optional — only validate when a value is actually provided.
/// </summary>
public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Dto.DisplayName)
            .MinimumLength(3).WithMessage("Display name must be at least 3 characters.")
            .MaximumLength(30).WithMessage("Display name cannot exceed 30 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.DisplayName));

        RuleFor(x => x.Dto.Bio)
            .MaximumLength(160).WithMessage("Bio cannot exceed 160 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Bio));

        RuleFor(x => x.Dto.Website)
            .MaximumLength(100).WithMessage("Website URL cannot exceed 100 characters.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Website must be a valid URL starting with http:// or https://.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Website));

        RuleFor(x => x.Dto.Location)
            .MaximumLength(100).WithMessage("Location cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Location));

        RuleFor(x => x.Dto.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{6,14}$")
            .WithMessage("Invalid phone number format. Use international format e.g. +201234567890.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.PhoneNumber));
    }
}