using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Specification.Conversations;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;
namespace Social_Media_Chatting_APP_Service.Features.Messages.Commands.MarkMessageAsRead;

public class MarkMessageAsReadCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRealtimeNotifier realtimeNotifier,
    DbContext context
) : IRequestHandler<MarkMessageAsReadCommand, Result<MarkMessageAsReadDto>>
{
    public async Task<Result<MarkMessageAsReadDto>> Handle(MarkMessageAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();
        var messagesRepo = unitOfWork.GetRepository<Message, Guid>();
        var ReadStatus = unitOfWork.GetRepository<MessageReadStatus>();

        var convoSpec = new ConversationSpecification(request.ConversationId);

        var conversation = await conversationRepo.FindAsync(convoSpec);

        if (conversation is null)
        {
            return Error.NotFound("Conversation.NotFound", "No conversation with that Id was found");
        }

        if (!conversation.Participants.Any(u => Guid.Parse(u.UserId) == request.RequesterId))
        {
            return Error.Forbidden("Message.NotParticipant", "You are not part of this conversation");
        }

        var boundaryMessage = await messagesRepo.GetByIdAsync(request.UpToMessageId);

        if (boundaryMessage == null)
        {
            return Error.NotFound("boundaryMessage.NotFound", "no Boundary Message was found");
        }

        if (boundaryMessage.ConversationId == request.ConversationId)
        {
            return Error.BadRequest("Message.InvalidBoundary", "Boundary message does not belong to this conversation");
        }

        // the part that we gonna know from it that we gonna mark all the messages before that as read
        var cutOff = boundaryMessage.SentAt;

        var readerIdString = request.RequesterId.ToString();

        var eligibleMessages = await messagesRepo.FindAllAsync(m =>
            m.ConversationId == request.ConversationId &&
            m.SentAt <= cutOff &&
            m.SenderId != readerIdString &&
            !m.IsDeleted);
        // we need to get the messages that already read by the user so we dont add it twice 
       
        var alreadyRead =
        .Where(mr => mr.MessageId == request.UpToMessageId && mr.ReaderId == readerIdString)
        .ToListAsync(cancellationToken);
     
    }
}