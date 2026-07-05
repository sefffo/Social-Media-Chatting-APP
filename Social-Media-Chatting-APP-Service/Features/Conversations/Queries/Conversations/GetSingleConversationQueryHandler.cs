using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Specification.Conversations;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Queries.Conversations;

public class GetSingleConversationQueryHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<GetSingleConversationQuery, Result<ConversationDto>>
{
    public async Task<Result<ConversationDto>> Handle(GetSingleConversationQuery request,
        CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();
        var convoSpec = new SingleConversationSpecification(request.ConversationId);
        var conversation = await conversationRepo.FindAsync(convoSpec);
        if (conversation == null || conversation.Participants.Count == 0)
        {
            return Error.NotFound("Conversation.NotFound", "Conversation Not found");
        }

        if (!conversation.Participants.Any(p => p.UserId == request.RequesterId.ToString()))
        {
            return Error.Forbidden("Conversation.NotParticipant", "You are not part of this conversation");
        }

        var isSelf = conversation.Participants.Count == 1 &&
                     conversation.Participants.First().UserId == request.RequesterId.ToString();
        var visibleParticipants = isSelf
            ? conversation.Participants
            : conversation.Participants.Where(p => p.UserId != request.RequesterId.ToString());

        var mappedParticipants = visibleParticipants
            .Select(p => new ParticipantDto
            {
                UserId = p.UserId,
                AvatarUrl = p.User.ProfilePicture,
                DisplayName = p.User.DisplayName ?? p.User.UserName ?? string.Empty,
                IsOnline = false
            }).ToList();
        var mappedConvo = new ConversationDto()
        {
            Id = conversation.Id,
            ConversationType = conversation.ConversationType,
            CreatedAt = conversation.CreatedAt,
            ImageUrl = conversation.ImageUrl,
            LastMessageAt = conversation.LastMessageAt,
            Name = conversation.Name,
            UnreadCount = conversation.Messages.Count(m => !m.IsDeleted
                                                           && m.SenderId != request.RequesterId.ToString()
                                                           && !m.ReadStatuses.Any(rs =>
                                                               rs.UserId == request.RequesterId.ToString())
            ),
            LastMessagePreview = conversation.LastMessage?.TextContent ??
                                 (conversation.LastMessage != null ? "📎 Attachment" : null),

            participant = mappedParticipants.FirstOrDefault(),

            OtherParticipant = mappedParticipants.ToList(),
        };

        return Result<ConversationDto>.Ok(mappedConvo);
        
    }
}
