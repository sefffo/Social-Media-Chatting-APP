using AutoMapper;
using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Specification.Conversations;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Queries.Conversations;

public class GetConversationsQueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRealtimeNotifier realtimeNotifier
    ) : IRequestHandler<GetConversationsQuery , Result<CursorPaginatedResult<ConversationDto>>>
{
    public async Task<Result<CursorPaginatedResult<ConversationDto>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();

        var convoSpec = new UserConversationSpecification(request.RequesterId , request.Before , request.PageSize);
        var conversations = await conversationRepo.FindAllAsync(convoSpec);

        foreach (var convo in conversations)
        {
            var mappedConvo = new ConversationDto()
            {
                Id = convo.Id,
                ConversationType = convo.ConversationType,
                CreatedAt = convo.CreatedAt,
                ImageUrl = convo.ImageUrl,
                Name = convo.Name,
                LastMessageAt = convo.LastMessageAt,
                UnreadCount = convo.
            };
        }
        
        
    }
}