using FluentValidation;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s;

namespace Social_Media_Chatting_APP_Service.Features.Authentication.Validators;

/// <summary>
/// Validates VerifyOtpDto before it reaches AuthService.
/// The DTO already has [Length(6,6)] via DataAnnotations but we mirror
/// it here with a Regex to also enforce digits-only — DataAnnotations
/// only checks length, not character set.
/// </summary>
public class VerifyOtpDtoValidator : AbstractValidator<VerifyOtpDto>
{
    public VerifyOtpDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("OTP code is required.")
            .Length(6).WithMessage("OTP code must be exactly 6 digits.")
            .Matches("^[0-9]{6}$").WithMessage("OTP code must contain digits only.");
    }
}
