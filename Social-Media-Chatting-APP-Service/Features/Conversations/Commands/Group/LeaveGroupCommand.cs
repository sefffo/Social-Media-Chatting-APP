using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Group;

public record LeaveGroupCommand(
    Guid RequesterId,
    Guid ConversationId
) : IRequest<Result<string>>;