using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Group;

public record UpdateGroupInfoCommand(
    Guid ConversationId,
    Guid RequesterId,
    string? Name,
    string? Description,
    string? ImageUrl
) : IRequest<Result<ConversationDto>>;