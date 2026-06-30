using MediatR;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Messages.Commands;

public record SendMessageCommand(
    string senderId, // from the JWT Token 
    Guid ConversationId,
    MessageContentType contentType,
    string? textContent,
    string? MediaUrl,
    string? FileName,
    Guid? ReplyToMessageId,
    string? mediaPublicId
)
    : IRequest<Result<MessageDto>>;
