using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Messages.Queries;

public record GetMessageQuery(Guid ConversationId , DateTime? Before , int PageSize , Guid RequesterId) : IRequest<Result<CursorPaginatedResult<MessageDto>>>;