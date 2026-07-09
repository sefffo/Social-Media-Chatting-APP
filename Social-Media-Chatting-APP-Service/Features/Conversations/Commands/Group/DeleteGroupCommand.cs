using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Commands.Group;

public record DeleteGroupCommand(
    Guid RequesterId,
    Guid ConversationId
) : IRequest<Result<string>>;