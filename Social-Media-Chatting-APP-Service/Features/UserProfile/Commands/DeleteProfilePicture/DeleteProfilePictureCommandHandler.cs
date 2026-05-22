using MediatR;
using Microsoft.AspNetCore.Identity;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Enums;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.UserProfile.Commands.DeleteProfilePicture;

public class DeleteProfilePictureCommandHandler(IUploadService uploadService, UserManager<AppUser> userManager)
    : IRequestHandler<DeleteProfilePictureCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }
        if (string.IsNullOrEmpty(user.ProfilePicturePublicId))
            return Result<string>.Fail(Error.BadRequest("User.NoPicture", "User has no profile picture to delete."));
        
        await uploadService.DeleteFileAsync(user.ProfilePicturePublicId, FileResourceType.Image);
        user.ProfilePicture = null;
        user.ProfilePicturePublicId = null;
        
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<string>.Fail(Error.Failure("User.UpdateFailed", result.Errors.First().Description));

        return Result<string>.Ok("Profile picture deleted successfully.");
    }
}