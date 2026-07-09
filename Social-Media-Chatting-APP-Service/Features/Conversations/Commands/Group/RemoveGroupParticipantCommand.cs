using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Group;

public record RemoveGroupParticipantCommand(
    Guid ConversationId,
    Guid RequesterId,
    Guid ParticipantId
    ) : IRequest<Result<string>>;