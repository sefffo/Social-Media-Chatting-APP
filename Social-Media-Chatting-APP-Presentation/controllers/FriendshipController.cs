using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_Presentation.Filters;
using Social_Media_Chatting_APP_Service.Features.Friendship.Blocking.BlockUser;
using Social_Media_Chatting_APP_Service.Features.Friendship.Blocking.UnblockUser;
using Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.RespondToFriendRequest;
using Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.SendFriendRequest;
using Social_Media_Chatting_APP_Service.Features.Friendship.Commands.MangeFriendship;
using Social_Media_Chatting_APP_Service.Features.Friendship.Queries.BlockedUser;
using Social_Media_Chatting_APP_Service.Features.Friendship.Queries.Friends;
using Social_Media_Chatting_APP_Service.Features.Friendship.Queries.PenddingRequests;
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
    [RedisCacheInvalidateWithResponse("friendship-requests")]
    public async Task<ActionResult<FriendshipActionResultDto>> RespondToFriendRequest(
        Guid friendshipId,
        [FromBody] RespondToFriendRequestDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(
            new RespondToFriendRequestCommand(userId, friendshipId, dto.Decision));
        return HandleResult(result);
    }

    [HttpDelete("unfriend/{targetUserId}")]
    [Authorize]
    [RedisCacheInvalidate("friendship-friends", "targetUserId")]
    public async Task<ActionResult<FriendshipActionResultDto>> UnfriendUser(Guid targetUserId)
    {
        var UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await sender.Send(new UnfriendCommand(UserId, targetUserId));
        return HandleResult(result);
    }

    [HttpPost("block/{targetUserId}")]
    [RedisCacheInvalidate("friendship-friends", "targetUserId")]
    [RedisCacheInvalidate("friendship-requests", "targetUserId")]
    [Authorize]
    public async Task<ActionResult<FriendshipActionResultDto>> BlockUser(Guid targetUserId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var Result = await sender.Send(new BlockUserCommand(userId, targetUserId));
        return HandleResult(Result);
    }

    [HttpDelete("unblock/{targetUserId}")]
    [RedisCacheInvalidate("friendship-blocked", "targetUserId")] //to check the block json cached 
    [Authorize]
    public async Task<ActionResult<FriendshipActionResultDto>> UnblockUser(Guid targetUserId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var Result = await sender.Send(new UnblockUserCommand(userId, targetUserId));
        return HandleResult(Result);
    }

    [HttpGet("friends")]
    [RedisCache("friendship-friends", ttlSeconds: 1000)]
    [Authorize]
    public async Task<ActionResult<IEnumerable<FriendListItemDto>>> GetFriends()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await sender.Send(new GetFriendsQuery(userId));
        return HandleResult(result);
    }

    [HttpGet("blocked")]
    [Authorize]
    [RedisCache("friendship-blocked", ttlSeconds: 1000)]
    public async Task<ActionResult<IEnumerable<BlockedUserItemDto>>> GetBlocked()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await sender.Send(new GetBlockedUsersQuery(userId));
        return HandleResult(result);
    }

    [HttpGet("requests/incoming")]
    [Authorize]
    [RedisCache("friendship-requests-incoming", ttlSeconds: 300)]
    public async Task<ActionResult<FriendshipActionResultDto>> GetFriendRequests()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await sender.Send(new PendingRequestsQuery(userId));


        if (!result.IsSuccess)
            return HandleResult(result);

        var incomingResult = result.GetValueOrThrow().Where(i => i.Direction == "Incoming"
        );

        return Ok(incomingResult);
    }

    [HttpGet("requests/outgoing")]
    [Authorize]
    [RedisCache("friendship-requests-outgoing", ttlSeconds: 300)]
    public async Task<ActionResult<IEnumerable<FriendRequestItemDto>>> GetOutgoingRequests()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new PendingRequestsQuery(userId));

        if (!result.IsSuccess)
            return HandleResult(result);

        var outgoing = result.GetValueOrThrow()
            .Where(r => r.Direction == "Outgoing");

        return Ok(outgoing);
    }
}