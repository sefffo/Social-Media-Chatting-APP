using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Specification.Conversations;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Commands.Group;

public class DeleteGroupHandler(
    IUnitOfWork unitOfWork,
    IRealtimeNotifier realtimeNotifier
) : IRequestHandler<DeleteGroupCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();

        var conversation = await conversationRepo.FindAsync(
            new ConversationSpecification(request.ConversationId));

        if (conversation is null)
            return Error.NotFound("Conversation.NotFound", "Group not found");

        if (conversation.ConversationType != ConvoType.Group)
            return Error.BadRequest("Conversation.NotGroup", "This operation only applies to groups");

        if (conversation.CreatedByUserId != request.RequesterId.ToString())
            return Error.Forbidden("Conversation.NotCreator", "Only the group creator can delete this group");

        var participantIds = conversation.Participants.Select(p => p.UserId).ToList();

        conversationRepo.Remove(conversation);
        await unitOfWork.SaveChangesAsync();

        foreach (var userId in participantIds)
        {
            await realtimeNotifier.NotifyGroupDeletedAsync(userId, request.ConversationId);
        }

        return Result<string>.Ok("Conversation deleted successfully");
    }
}