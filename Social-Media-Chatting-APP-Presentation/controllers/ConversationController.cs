using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_Service.Features.Conversations.CreateDMConversation;
using Social_Media_Chatting_APP_Service.Features.Conversations.Group;
using Social_Media_Chatting_APP_Service.Features.Conversations.Queries.Conversations;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationController(
    ISender sender
) : ApiBaseController
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<CursorPaginatedResult<ConversationDto>>> GetConversations(
        [FromQuery] DateTime? before,
        [FromQuery] int pageSize = 20)
    {
        var user = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await sender.Send(new GetConversationsQuery(user, before, pageSize));
        return HandleResult(result);
    }

    [Authorize]
    [HttpGet("{conversationId}")]
    public async Task<ActionResult<ConversationDto>> GetConversation(Guid conversationId)
    {
        var user = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await sender.Send(new GetSingleConversationQuery(user, conversationId));
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("dm")]
    public async Task<ActionResult<ConversationDto>> CreateDmConversation([FromQuery] Guid targetUserId)
    {
        var user = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await sender.Send(new CreateDmConversationCommand(user, targetUserId));
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("group")]
    public async Task<ActionResult<ConversationDto>> CreateGroupConversation([FromQuery] string name,
        List<Guid> targetUserIds, string? imageUrl, string? description)
    {
        var user = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await sender.Send(new CreateGroupConversation(user, name, targetUserIds, imageUrl, description));
        return HandleResult(result);
    }
}