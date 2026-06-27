using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Server.HttpSys;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Friendship.Helpers;
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
        var convoRepo = unitOfWork.GetRepository<Conversation, Guid>();
        var messageRepo = unitOfWork.GetRepository<Message, Guid>();
        var friendshipRepo = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();


        // now we need to fetch all the convos 
        var convSpec = new ConversationSpecification(request.ConversationId);
        var conversation = await convoRepo.FindAsync(convSpec);
        if (conversation is null) return Error.NotFound("Conversation.NotFound", "No conversation found with this id");

        if (!conversation.Participants.Any(u => (u.UserId) == request.senderId))
        {
            return Error.Forbidden("Message.NotParticipant", "You are not part of this conversation");
        }

        var friendshipExists = await FriendshipQueryHelper.GetAsync(friendshipRepo, Guid.Parse(request.senderId),
            Guid.Parse(conversation.Participants.First(p => p.UserId == request.senderId).UserId));

        if (friendshipExists == null || friendshipExists.Status != FriendshipStatus.Accepted)
        {
            return Error.Forbidden("Message.NotFriends", "You are not friends with this user");
        }


        return default;
    }
}