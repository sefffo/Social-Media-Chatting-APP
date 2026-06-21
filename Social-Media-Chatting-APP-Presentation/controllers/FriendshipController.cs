using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_Presentation.Filters;
using Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.RespondToFriendRequest;
using Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.SendFriendRequest;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;

namespace Social_Media_Chatting_APP_Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FriendshipController(ISender sender) : ApiBaseController
{
    [HttpPost("send/{targetUserId}")]
    [Authorize]
    [RedisCacheInvalidate("friendship-requests", "targetUserId")]
    public async Task<ActionResult<FriendshipActionResultDto>> SendFriendRequest(Guid targetUserId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await sender.Send(new SendFriendRequestCommand(userId, targetUserId));
        return HandleResult(result);
    }

    [HttpPut("respond/{friendshipId}")]
    [Authorize]
    [RedisCacheInvalidate("friendship-requests")]
    public async Task<ActionResult<FriendshipActionResultDto>> RespondToFriendRequest(
        Guid friendshipId,
        [FromBody] RespondToFriendRequestDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new RespondToFriendRequestCommand(userId, friendshipId, dto.Decision));
        return HandleResult(result);
    }
}