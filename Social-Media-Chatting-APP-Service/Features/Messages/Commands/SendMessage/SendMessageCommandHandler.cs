using AutoMapper;
using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Specification.Conversations;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Messages.Commands;

public class SendMessageCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRealtimeNotifier realtimeNotifier // used for teh events 
) : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var convoRepo = unitOfWork.GetRepository<Message, Guid>();
        var messageRepo = unitOfWork.GetRepository<Message, Guid>();
        var friendshipRep0 = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();
        
        
        // now we need to fetch all the convos 
        var convSpec = new ConversationSpecification(request.ConversationId);
        var conversation =  await convoRepo.FindAsync(convSpec);
        
        
        


        return default;
    }
}