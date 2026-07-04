using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Friendship.Helpers;
using Social_Media_Chatting_APP_Service.Specification.Conversations;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.CreateDMConversation;

public class CreateDmConversationCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRealtimeNotifier realtimeNotifier,
    UserManager<AppUser> userManager
) : IRequestHandler<CreateDmConversationCommand, Result<ConversationDto>>
{
    public async Task<Result<ConversationDto>> Handle(CreateDmConversationCommand request,
        CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();
        var friendshipRepo = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();
        bool isSelfDM = request.OtherUser == request.RequesterId;
        var conversation =
            await conversationRepo.FindAsync(
                new ExistingConversationSpecification(request.RequesterId, request.OtherUser));
        if (!isSelfDM)
        {
            var otherUser = await userManager.FindByIdAsync(request.OtherUser.ToString());
            if (otherUser is null)
            {
                return Error.NotFound("User.NotFound", "User not found");
            }

            var friendshipExists =
                await FriendshipQueryHelper.GetAsync(friendshipRepo, request.RequesterId, request.OtherUser);
            if (friendshipExists is null || friendshipExists.Status != FriendshipStatus.Accepted)
            {
                return Error.Forbidden("Conversation.NotFriends", "You must be friends to start a DM");
            }

            
            if (conversation is null)
            {
                var newConversation = new Conversation()
                {
                    Id = Guid.NewGuid(),
                    ConversationType = ConvoType.DirectMessage,
                    CreatedAt =DateTime.UtcNow,
                    Participants = new List<ConversationParticipant>()
                    {
                        new() {UserId = request.RequesterId.ToString()},
                        new() {UserId = request.OtherUser.ToString()}
                    },
                };
                
                await conversationRepo.AddAsync(newConversation);
                await unitOfWork.SaveChangesAsync();
                var newMappedConvo = mapper.Map<ConversationDto>(newConversation);
                await realtimeNotifier.NotifyNewConversationAsync(request.OtherUser.ToString(), newMappedConvo);
                return Result<ConversationDto>.Ok(newMappedConvo);
            }
            
        }  else if (conversation is null)
        {
            var newSelfConvo = new Conversation
            {
                Id = Guid.NewGuid(),
                ConversationType = ConvoType.DirectMessage,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.RequesterId.ToString(),
                Participants = new List<ConversationParticipant>
                {
                    new() { UserId = request.RequesterId.ToString() }
                }
            };
            await conversationRepo.AddAsync(newSelfConvo);
            await unitOfWork.SaveChangesAsync();
            var selfDto = mapper.Map<ConversationDto>(newSelfConvo);
            return Result<ConversationDto>.Ok(selfDto);
        }
        var mappedConvo = mapper.Map<ConversationDto>(conversation);
        return Result<ConversationDto>.Ok(mappedConvo);
    }
}