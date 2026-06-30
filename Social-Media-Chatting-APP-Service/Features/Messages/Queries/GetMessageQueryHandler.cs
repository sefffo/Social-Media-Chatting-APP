using AutoMapper;
using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Specification.Conversations;
using Social_Media_Chatting_APP_Service.Specification.Messages;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Messages.Queries;

public class GetMessageQueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRealtimeNotifier realtimeNotifier
) : IRequestHandler<GetMessageQuery, Result<CursorPaginatedResult<MessageDto>>>
{
    public async Task<Result<CursorPaginatedResult<MessageDto>>> Handle(GetMessageQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ConversationId == null)
        {
            return Error.NotFound("Conversation.Notfound", "No Conversation Found");
        }

        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();
        var messagesRepo = unitOfWork.GetRepository<Message, Guid>();
        //get that convo needed by the Spec to get the participants of that conversation 

        var convoSpec = new ConversationSpecification(request.ConversationId);
        var conversation = await conversationRepo.GetByIdAsync(convoSpec);
        if (conversation is null)
            return Error.NotFound("Conversation.NotFound", "No conversation found");
        if (!conversation.Participants.Any(cp => Guid.Parse(cp.UserId) == request.RequesterId
            ))
        {
            return Error.Forbidden("Message.NotFriends",
                "U can't send messages to other users That is not participants of this conversation");
        }

        var messageSpc = new MessagesSpecifications(request.ConversationId, request.Before, request.PageSize);


        var paginatedResult =(await messagesRepo.FindAllAsync(messageSpc)).ToList();


        // if (paginatedResult.Count() > request.PageSize)
        // {
        //     paginatedResult.RemoveAt(paginatedResult.Count - 1);
        // }

        var mappedMessage = new List<MessageDto>();
        foreach (var message in paginatedResult)
        {
            var dto = mapper.Map<MessageDto>(message);
            dto.IsRead = message.ReadStatuses.Any(rs => Guid.Parse(rs.UserId) == request.RequesterId);
            dto.IsReply = message.ReplyToMessageId != null;


            mappedMessage.Add(dto);
        }

        var cursorResult = CursorPaginatedResult<MessageDto>.Create(mappedMessage, request.PageSize, m => m.SentAt);
        return cursorResult;
    }
}