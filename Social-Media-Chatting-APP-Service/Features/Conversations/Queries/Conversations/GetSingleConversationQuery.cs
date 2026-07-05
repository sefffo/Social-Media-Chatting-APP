using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Queries.Conversations;

public record GetSingleConversationQuery(Guid RequesterId , Guid ConversationId) : IRequest<Result<ConversationDto>>;