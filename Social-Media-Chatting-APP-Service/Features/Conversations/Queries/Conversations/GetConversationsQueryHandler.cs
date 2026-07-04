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
    IUnitOfWork unitOfWork
) : IRequestHandler<GetConversationsQuery, Result<CursorPaginatedResult<ConversationDto>>>
{
    public async Task<Result<CursorPaginatedResult<ConversationDto>>> Handle(GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();

        var convoSpec = new UserConversationSpecification(request.RequesterId, request.Before, request.PageSize);
        var conversations = (await conversationRepo.FindAllAsync(convoSpec)).ToList();


        var dtos = new List<ConversationDto>();

        foreach (var convo in conversations)
        {
            var isSelfDm = convo.Participants.Count == 1 &&
                           convo.Participants.First().UserId == request.RequesterId.ToString();

            var visibleParticipants = isSelfDm
                ? convo.Participants
                : convo.Participants.Where(p => p.UserId != request.RequesterId.ToString());

            var mappedConvo = new ConversationDto()
            {
                Id = convo.Id,
                ConversationType = convo.ConversationType,
                CreatedAt = convo.CreatedAt,
                ImageUrl = convo.ImageUrl,
                Name = convo.Name,
                LastMessageAt = convo.LastMessageAt,
                //m3naha ay message msh mwgoda fe le readstatus table m3naha en el user da mshafhash 
                UnreadCount = convo.Messages.Count(m =>
                    !m.IsDeleted &&
                    m.SenderId !=
                    request.RequesterId.ToString() // make sure to not count the user messages in the unread 
                    && !m.ReadStatuses.Any(rs => rs.UserId == request.RequesterId.ToString())),
                LastMessagePreview = convo.LastMessage?.TextContent ??
                                     (convo.LastMessage != null ? "📎 Attachment" : null),

                OtherParticipant = visibleParticipants
                    .Select(p => new ParticipantDto
                    {
                        UserId = p.UserId,
                        DisplayName = p.User.DisplayName ?? p.User.UserName ?? string.Empty,
                        AvatarUrl = p.User.ProfilePicture,
                        IsOnline = false // populated later by PresenceTracker
                    })
                    .ToList()
            };

            dtos.Add(mappedConvo);
        }

        var result = CursorPaginatedResult<ConversationDto>.Create(dtos, request.PageSize,
            mappedConvo => mappedConvo.LastMessageAt ?? mappedConvo.CreatedAt);

        return Result<CursorPaginatedResult<ConversationDto>>.Ok(result);
    }
}