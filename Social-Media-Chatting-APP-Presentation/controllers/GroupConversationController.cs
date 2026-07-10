using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_Service.Features.Conversations.Commands.Group;
using Social_Media_Chatting_APP_Service.Features.Conversations.Group;
using Social_Media_Chatting_APP_Service.Features.Conversations.Group.ChangeRole;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;

namespace Social_Media_Chatting_APP_Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupConversationController(
    ISender sender
) : ApiBaseController
{
    [Authorize]
    [HttpPost("{conversationId:guid}/participants")]
    public async Task<IActionResult> AddParticipants(
        Guid conversationId,
        [FromBody] AddGroupParticipantRequestDto dto)
    {
        var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new AddGroupParticipantCommand(
            requesterId, conversationId, dto.NewParticipantIds));
        return HandleResult(result);
    }

    [Authorize]
    [HttpDelete("{conversationId:guid}/participants/{participantId:guid}")]
    public async Task<IActionResult> RemoveParticipant(
        Guid conversationId,
        Guid participantId)
    {
        var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new RemoveGroupParticipantCommand(
            requesterId, conversationId, participantId));
        return HandleResult(result);
    }

    [Authorize]
    [HttpPatch("{conversationId:guid}/info")]
    public async Task<IActionResult> UpdateGroupInfo(
        Guid conversationId,
        [FromBody] UpdateGroupInfoRequestDto dto)
    {
        var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new UpdateGroupInfoCommand(
            conversationId, requesterId, dto.Name, dto.Description, dto.ImageUrl));
        return HandleResult(result);
    }

    [Authorize]
    [HttpPatch("{conversationId:guid}/role")]
    public async Task<IActionResult> ChangeParticipantRole(
        Guid conversationId,
        [FromBody] ChangeGroupRoleRequestDto dto)
    {
        var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new ChangeGroupRoleCommand(
            requesterId, conversationId, dto.TargetUserId, dto.NewRole));
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("{conversationId:guid}/leave")]
    public async Task<IActionResult> LeaveGroup(Guid conversationId)
    {
        var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new LeaveGroupCommand(requesterId, conversationId));
        return HandleResult(result);
    }

    [Authorize]
    [HttpDelete("{conversationId:guid}")]
    public async Task<IActionResult> DeleteGroup(Guid conversationId)
    {
        var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new DeleteGroupCommand(requesterId, conversationId));
        return HandleResult(result);
    }
}