using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Messages.Commands.MarkMessageAsRead;

public record MarkMessageAsReadCommand(Guid ConversationId ,Guid RequesterId ,Guid UpToMessageId) : IRequest<Result<MarkMessageAsReadDto>>;