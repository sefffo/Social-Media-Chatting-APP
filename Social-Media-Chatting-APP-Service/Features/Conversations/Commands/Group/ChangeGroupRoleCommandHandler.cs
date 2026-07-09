using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Conversations.Group.Helpers;
using Social_Media_Chatting_APP_Service.Specification.Conversations;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Group.ChangeRole;

public class ChangeGroupRoleHandler(
    IUnitOfWork unitOfWork,
    IRealtimeNotifier realtimeNotifier
) : IRequestHandler<ChangeGroupRoleCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ChangeGroupRoleCommand request, CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();

        var conversation = await conversationRepo.FindAsync(
            new ConversationSpecification(request.ConversationId));

        if (conversation is null)
            return Error.NotFound("Conversation.NotFound", "Group not found");

        if (conversation.ConversationType != ConvoType.Group)
            return Error.BadRequest("Conversation.NotGroup", "This operation only applies to groups");

        if (!GroupPermissionHelper.IsGroupAdmin(conversation, request.RequesterId.ToString()))
            return Error.Forbidden("Conversation.NotAdmin", "Only group admins can change member roles");

        var targetParticipant = conversation.Participants
            .FirstOrDefault(p => p.UserId == request.TargetUserId.ToString());

        if (targetParticipant is null)
            return Error.NotFound("Conversation.NotMember", "User is not a member of this group");

        if (request.NewRole == GroupRole.GroupMember &&
            targetParticipant.Role == GroupRole.GroupAdmin &&
            conversation.Participants.Count(p => p.Role == GroupRole.GroupAdmin) == 1)
        {
            return Error.BadRequest("Conversation.LastAdmin",
                "Cannot demote the only remaining admin. Promote another member first");
        }

        targetParticipant.Role = request.NewRole;
        conversationRepo.Update(conversation);
        await unitOfWork.SaveChangesAsync();

        await realtimeNotifier.NotifyRoleChangedAsync(request.TargetUserId.ToString(), request.ConversationId, request.NewRole);

        return Result<string>.Ok("Role changed successfully");
    }
}