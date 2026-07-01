using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.CreateDMConversation;

// he one initiating the convo 
public record CreateDmConversationCommand(Guid RequesterId, Guid OtherUser) : IRequest<Result<ConversationDto>>;