using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_Service.Features.Messages.Commands;
using Social_Media_Chatting_APP_Service.Features.Messages.Commands.MarkMessageAsRead;
using Social_Media_Chatting_APP_Service.Features.Messages.Queries;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController(
    ISender sender
) : ApiBaseController
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] SendMessageRequestDto dto)
    {
        var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new SendMessageCommand(
            senderId,
            dto.ConversationId,
            dto.ContentType,
            dto.TextContent,
            dto.MediaUrl,
            dto.FileName,
            dto.ReplyToMessageId,
            dto.MediaPublicId
        ));
        return HandleResult(result);
    }

    [Authorize]
    [HttpGet("{conversationId:guid}")]
    public async Task<ActionResult<CursorPaginatedResult<MessageDto>>> GetMessages(
        Guid conversationId,
        [FromQuery] DateTime? before,
        [FromQuery] int pageSize = 20)
    {
        var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new GetMessageQuery(conversationId, before, pageSize, requesterId));
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("read")]
    public async Task<ActionResult<MarkMessageAsReadDto>> MarkAsRead([FromBody] MarkAsReadRequestDto dto)
    {
        var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new MarkMessageAsReadCommand(dto.ConversationId, requesterId, dto.UpToMessageId));
        return HandleResult(result);
    }
}
