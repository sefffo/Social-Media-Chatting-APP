using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Group.ChangeRole;

public record ChangeGroupRoleCommand(
    Guid RequesterId,
    Guid ConversationId,
    Guid TargetUserId,
    GroupRole NewRole
) : IRequest<Result<string>>;