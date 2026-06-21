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
    [RedisCacheInvalidate("friendship-requests-incoming", "targetUserId")]
    [RedisCacheInvalidate("friendship-requests-outgoing", "targetUserId")]
    public async Task<ActionResult<FriendshipActionResultDto>> SendFriendRequest(Guid targetUserId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new SendFriendRequestCommand(userId, targetUserId));
        return HandleResult(result);
    }

    [HttpPut("respond/{friendshipId}")]
    [Authorize]
    [RedisCacheInvalidateWithResponse("friendship-requests-incoming")]
    [RedisCacheInvalidateWithResponse("friendship-requests-outgoing")]
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
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new UnfriendCommand(userId, targetUserId));
        return HandleResult(result);
    }

    [HttpPost("block/{targetUserId}")]
    [Authorize]
    [RedisCacheInvalidate("friendship-requests-incoming", "targetUserId")]
    [RedisCacheInvalidate("friendship-requests-outgoing", "targetUserId")]
    [RedisCacheInvalidate("friendship-friends", "targetUserId")]
    public async Task<ActionResult<FriendshipActionResultDto>> BlockUser(Guid targetUserId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new BlockUserCommand(userId, targetUserId));
        return HandleResult(result);
    }

    [HttpDelete("unblock/{targetUserId}")]
    [Authorize]
    [RedisCacheInvalidate("friendship-blocked", "targetUserId")]
    public async Task<ActionResult<FriendshipActionResultDto>> UnblockUser(Guid targetUserId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new UnblockUserCommand(userId, targetUserId));
        return HandleResult(result);
    }

    [HttpGet("friends")]
    [Authorize]
    [RedisCache("friendship-friends", ttlSeconds: 1000)]
    public async Task<ActionResult<IEnumerable<FriendListItemDto>>> GetFriends()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new GetFriendsQuery(userId));
        return HandleResult(result);
    }

    [HttpGet("blocked")]
    [Authorize]
    [RedisCache("friendship-blocked", ttlSeconds: 1000)]
    public async Task<ActionResult<IEnumerable<BlockedUserItemDto>>> GetBlocked()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new GetBlockedUsersQuery(userId));
        return HandleResult(result);
    }

    [HttpGet("requests/incoming")]
    [Authorize]
    [RedisCache("friendship-requests-incoming", ttlSeconds: 300)]
    public async Task<ActionResult<IEnumerable<FriendRequestItemDto>>> GetIncomingRequests()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new PendingRequestsQuery(userId));

        if (!result.IsSuccess)
            return HandleResult(result);

        var incoming = result.GetValueOrThrow()
            .Where(r => r.Direction == "Incoming");

        return Ok(incoming);
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