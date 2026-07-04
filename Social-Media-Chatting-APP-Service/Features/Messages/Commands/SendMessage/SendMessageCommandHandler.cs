using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Server.HttpSys;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
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

        // to check if the JWT token of the Sender is Valid 
        if (!Guid.TryParse(request.senderId, out var senderGuid))
            return Error.BadRequest("Message.InvalidSender", "Invalid sender ID");

        if (conversation.ConversationType == ConvoType.DirectMessage)
        {
            
            var isSelf = conversation.Participants.Count == 1 && conversation.Participants.First().UserId == request.senderId;
            // we dont need to check if the user is friends if it's a self dm'
            if (isSelf)
            {
                var selfMessage = new Message()
                {
                    Id = Guid.NewGuid(),
                    ContentType = request.contentType,
                    ConversationId = request.ConversationId,
                    SenderId = request.senderId,
                    MediaUrl = request.MediaUrl,
                    FileName = request.FileName,
                    IsDeleted = false,
                    SentAt = DateTime.UtcNow,
                    TextContent = request.textContent,
                    ReplyToMessageId = request.ReplyToMessageId,
                    MediaPublicId = request.mediaPublicId,
                };

                conversation.LastMessageAt = selfMessage.SentAt;
                conversation.LastMessageId = selfMessage.Id;

                convoRepo.Update(conversation);
                await messageRepo.AddAsync(selfMessage);
                await unitOfWork.SaveChangesAsync();

                var mappedSelfMessage = mapper.Map<MessageDto>(selfMessage);
                await realtimeNotifier.BroadcastNewMessage(request.ConversationId, mappedSelfMessage);

                return Result<MessageDto>.Ok(mappedSelfMessage);
            }
            
            var friendshipExists = await FriendshipQueryHelper.GetAsync(friendshipRepo, senderGuid,
                Guid.Parse(conversation.Participants.First(u => u.UserId != request.senderId).UserId));
            if (friendshipExists == null || friendshipExists.Status != FriendshipStatus.Accepted)
            {
                return Error.Forbidden("Message.NotFriends", "You are not friends with this user");
            }
        }

        // Text messages must have content
        if (request.contentType == MessageContentType.Text &&
            string.IsNullOrWhiteSpace(request.textContent))
        {
            return Error.BadRequest("Message.EmptyText", "Text message cannot be empty");
        }

        // Media messages must have URL and public ID
        if ((request.contentType == MessageContentType.Image ||
             request.contentType == MessageContentType.Document ||
             request.contentType == MessageContentType.Video)
            && (string.IsNullOrWhiteSpace(request.MediaUrl) || request.mediaPublicId is null))
        {
            return Error.BadRequest("Message.MissingMedia", "Media URL and public ID are required");
        }

        // Document messages additionally need a file name
        if (request.contentType == MessageContentType.Document &&
            string.IsNullOrWhiteSpace(request.FileName))
        {
            return Error.BadRequest("Message.MissingFileName", "File name is required for document messages");
        }

        var message = new Message()
        {
            Id = Guid.NewGuid(),
            ContentType = request.contentType,
            ConversationId = request.ConversationId,
            SenderId = request.senderId,
            MediaUrl = request.MediaUrl,
            FileName = request.FileName,
            IsDeleted = false,
            SentAt = DateTime.UtcNow,
            TextContent = request.textContent,
            ReplyToMessageId = request.ReplyToMessageId,
            MediaPublicId = request.mediaPublicId,
        };

        conversation.LastMessageAt = message.SentAt;
        conversation.LastMessageId = message.Id;

        convoRepo.Update(conversation);
        await messageRepo.AddAsync(message);
        await unitOfWork.SaveChangesAsync();


        var mappedMessage = mapper.Map<MessageDto>(message);

        await realtimeNotifier.BroadcastNewMessage(request.ConversationId, mappedMessage);

        return Result<MessageDto>.Ok(mappedMessage);
    }
}