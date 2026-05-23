using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_Service.Features.UserProfile.Commands.DeleteProfilePicture;
using Social_Media_Chatting_APP_Service.Features.UserProfile.Commands.UpdateProfile;
using Social_Media_Chatting_APP_Service.Features.UserProfile.Commands.UploadProfilePicture;
using Social_Media_Chatting_APP_Service.Features.UserProfile.Queries.GetMyProfile;
using Social_Media_Chatting_APP_Service.Features.UserProfile.Quries.GetUserById;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;

namespace Social_Media_Chatting_APP_Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserProfileController(
    ISender sender
) : ApiBaseController
{
    [Authorize]
    [HttpGet("my-profile")]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new GetMyProfileQuery(userId));
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpGet("{userId}")]
    public async Task<ActionResult<PublicUserProfileDto>> GetUserProfile(Guid userId)
    {
        var result = await sender.Send(new GetUserByIdQuery(userId));
        return HandleResult(result);
    }

    [Authorize]
    [HttpPut("my-profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new UpdateProfileCommand(userId, dto));
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("my-profile/upload-picture")]
    public async Task<ActionResult<string>> UploadProfilePicture([FromForm] IFormFile file)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new UploadProfilePictureCommand(userId, file));
        return HandleResult(result);
    }

    [Authorize]
    [HttpDelete("my-profile/picture")]
    public async Task<ActionResult<string>> DeleteProfilePicture()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new DeleteProfilePictureCommand(userId));
        return HandleResult(result);
    }
}