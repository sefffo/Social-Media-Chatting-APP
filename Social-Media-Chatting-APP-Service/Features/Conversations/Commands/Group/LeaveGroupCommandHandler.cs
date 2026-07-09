using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Conversations.Group;
using Social_Media_Chatting_APP_Service.Specification.Conversations;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Commands.Group;

public class LeaveGroupHandler(
    IUnitOfWork unitOfWork,
    IRealtimeNotifier realtimeNotifier
) : IRequestHandler<LeaveGroupCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LeaveGroupCommand request, CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();

        var conversation = await conversationRepo.FindAsync(
            new ConversationSpecification(request.ConversationId));

        if (conversation is null)
            return Error.NotFound("Conversation.NotFound", "Group not found");

        if (conversation.ConversationType != ConvoType.Group)
            return Error.BadRequest("Conversation.NotGroup", "This operation only applies to groups");

        var participant = conversation.Participants
            .FirstOrDefault(p => p.UserId == request.RequesterId.ToString());

        if (participant is null)
            return Error.BadRequest("Conversation.NotMember", "You are not a member of this group");

        if (participant.Role == GroupRole.GroupAdmin &&
            conversation.Participants.Count(p => p.Role == GroupRole.GroupAdmin) == 1 &&
            conversation.Participants.Count > 1)
        {
            return Error.BadRequest("Conversation.LastAdmin",
                "You are the only admin. Promote another member before leaving");
        }

        conversation.Participants.Remove(participant);
        conversationRepo.Update(conversation);
        await unitOfWork.SaveChangesAsync();

        await realtimeNotifier.NotifyGroupUpdatedAsync(request.RequesterId.ToString(),
            null); // could pass a lightweight notification instead

        return Result<string>.Ok("Left group successfully");
    }
}