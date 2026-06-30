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

/// <summary>
/// how it works ?
///first we need to get the messages that are before the boundary message ==> and its the message which cuts off between
///the messages that are already read by the user and the ones are not.
///then we need to get the messages that are already read by the user
///so we can exclude them so we dont make double insertion in the ReadStatus table 
///then we need to insert these messages into the database
/// Note we created a special repository for this purpose as we cant follow the same pattern of UOF here
/// </summary>
/// <param name="unitOfWork"></param>
/// <param name="mapper"></param>
/// <param name="realtimeNotifier"></param>
/// <param name="messageReadStatusRepository"></param>
public class MarkMessageAsReadCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRealtimeNotifier realtimeNotifier,
    IMessageReadStatusRepository messageReadStatusRepository
) : IRequestHandler<MarkMessageAsReadCommand, Result<MarkMessageAsReadDto>>
{
    public async Task<Result<MarkMessageAsReadDto>> Handle(MarkMessageAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();
        var messagesRepo = unitOfWork.GetRepository<Message, Guid>();


        var convoSpec = new ConversationSpecification(request.ConversationId);

        var conversation = await conversationRepo.FindAsync(convoSpec);

        if (conversation is null)
        {
            return Error.NotFound("Conversation.NotFound", "No conversation with that Id was found");
        }

        var requesterIdString = request.RequesterId.ToString();

        if (!conversation.Participants.Any(u => u.UserId == requesterIdString))
        {
            return Error.Forbidden("Message.NotParticipant", "You are not part of this conversation");
        }

        var boundaryMessage = await messagesRepo.GetByIdAsync(request.UpToMessageId);

        if (boundaryMessage == null)
        {
            return Error.NotFound("boundaryMessage.NotFound", "no Boundary Message was found");
        }

        if (boundaryMessage.ConversationId != request.ConversationId)
        {
            return Error.BadRequest("Message.InvalidBoundary", "Boundary message does not belong to this conversation");
        }

        // the part that we gonna know from it that we gonna mark all the messages before that as read
        var cutOff = boundaryMessage.SentAt;

        var readerIdString = request.RequesterId.ToString();

        var eligibleMessages = await messagesRepo.FindAllAsync(m =>
            m.ConversationId == request.ConversationId &&
            m.SentAt <= cutOff &&
            m.SenderId != readerIdString && // not sent by the sender (cuz we already read it)
            !m.IsDeleted);
        var candidatesId = eligibleMessages.Select(m => m.Id).ToList();
        // we need to get the messages that already read by the user so we don't add it twice 
    
        //short the results if its empty and he already read all the messages 
        if (!candidatesId.Any())
        {
            var emptyResult = new MarkMessageAsReadDto
            {
                ConversationId = request.ConversationId,
                ReaderId = request.RequesterId,
                UpToMessageId = request.UpToMessageId,
                NewlyMessageIds = [],
                ReadAt = DateTime.UtcNow
            };

            // You can still broadcast if you want, but usually no need when list is empty
            return Result<MarkMessageAsReadDto>.Ok(emptyResult);
        }


        var alreadyReadIds =
            await messageReadStatusRepository.GetAlreadyReadMessageIdsAsync(request.RequesterId, candidatesId);

        var newlyRead = candidatesId.Except(alreadyReadIds).ToList();
        var readAt = DateTime.UtcNow;
        // now we need to insert these messages into the database 


        await messageReadStatusRepository.AddReadStatusesAsync(request.RequesterId, newlyRead, readAt);


        await unitOfWork.SaveChangesAsync();

        var result = new MarkMessageAsReadDto()
        {
            ConversationId = request.ConversationId,
            ReaderId = request.RequesterId,
            UpToMessageId = request.UpToMessageId,
            NewlyMessageIds = newlyRead,
            ReadAt = readAt
        };
        //the most important we need to broadcast that 

        await realtimeNotifier.BroadcastReadReceipt(request.ConversationId, result.ReaderId, newlyRead);

        return Result<MarkMessageAsReadDto>.Ok(result);
    }
}