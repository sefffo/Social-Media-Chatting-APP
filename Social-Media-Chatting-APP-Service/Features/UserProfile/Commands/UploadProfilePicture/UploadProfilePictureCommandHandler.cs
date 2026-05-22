using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Enums;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.UserProfile.Commands.UploadProfilePicture;

public class UploadProfilePictureCommandHandler(
    IUploadService uploadService,
    UserManager<AppUser> userManager
) : IRequestHandler<UploadProfilePictureCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UploadProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }

        //check if he already has a photo 
        // Step 1 — delete old picture if exists
        if (!string.IsNullOrEmpty(user.ProfilePicturePublicId))
        {
            await uploadService.DeleteFileAsync(user.ProfilePicturePublicId, FileResourceType.Image);
        }

        var uploadResult =
            await uploadService.UploadFileAsync(request.File, "profile-pictures", FileResourceType.Image);

        if (!uploadResult.IsSuccess)
            return Result<string>.Fail(Error.BadRequest("ImageUpload.BadRequest",
                uploadResult.Errors.First().Description));

        var uploaded = uploadResult.GetValueOrThrow();
        // Step 3 — update user fields
        user.ProfilePicture = uploaded.Url;
        user.ProfilePicturePublicId = uploaded.PublicId;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<string>.Fail(Error.Failure("User.UpdateFailed", result.Errors.First().Description));

        return Result<string>.Ok(user.ProfilePicture);
    }
}