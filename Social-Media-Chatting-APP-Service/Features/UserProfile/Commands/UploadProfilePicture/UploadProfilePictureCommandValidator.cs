using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Social_Media_Chatting_APP_Service.Features.UserProfile.Commands.UploadProfilePicture;

/// <summary>
/// Validates UploadProfilePictureCommand before it reaches the handler.
/// </summary>
public class UploadProfilePictureCommandValidator : AbstractValidator<UploadProfilePictureCommand>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UploadProfilePictureCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("Please provide an image file.");

        When(x => x.File != null, () =>
        {
            RuleFor(x => x.File.Length)
                .GreaterThan(0).WithMessage("The uploaded file is empty.")
                .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage("File size cannot exceed 5 MB.");

            RuleFor(x => x.File.FileName)
                .NotEmpty().WithMessage("File name is missing.")
                .Must(name =>
                {
                    var ext = Path.GetExtension(name)?.ToLowerInvariant();
                    return !string.IsNullOrEmpty(ext) && AllowedExtensions.Contains(ext);
                })
                .WithMessage($"Only the following image formats are allowed: {string.Join(", ", AllowedExtensions)}.");

            RuleFor(x => x.File.ContentType)
                .Must(ct => AllowedContentTypes.Contains(ct?.ToLowerInvariant()))
                .WithMessage("Invalid file content type. Allowed: image/jpeg, image/png, image/webp.");
        });
    }
}