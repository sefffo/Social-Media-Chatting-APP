using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Friendship.Helpers;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Group;

public class CreateGroupConversationHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRealtimeNotifier realtimeNotifier,
    UserManager<AppUser> userManager
) : IRequestHandler<CreateGroupConversation, Result<ConversationDto>>
{
    public async Task<Result<ConversationDto>> Handle(CreateGroupConversation request,
        CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();
        var friendshipRepo = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();
        
        var participantIds = request.ParticipantsIds
            .Where(id => id != request.RequesterId)
            .Distinct()
            .ToList();
        
        if (participantIds.Count == 0)
        {
            return Error.BadRequest("Conversation.NoParticipants",
                "A group requires at least one other participant");
        }
        var checkedUser = new List<AppUser>();
        foreach (var participantId in participantIds) 
        {
            var user = await userManager.FindByIdAsync(participantId.ToString());
            if (user is null)
            {
                return Error.NotFound("User.NotFound", $"User {participantId} not found");
            }

            checkedUser.Add(user);
        }

        foreach (var participantsId in checkedUser)
        {
            var user = await FriendshipQueryHelper.GetAsync(friendshipRepo, request.RequesterId,
                Guid.Parse(participantsId.Id));
            if (user is null || user.Status != FriendshipStatus.Accepted)
            {
                return Error.Forbidden("Conversation.NotFriends",
                    $"User {participantsId.UserName} is not your friend");
            }
        }

        var conversationParticipants = new List<ConversationParticipant>()
        {
            new()
            {
                UserId = request.RequesterId.ToString(),
                Role = GroupRole.GroupAdmin
            }
        };
        foreach (var groupMember in checkedUser) 
        {
            conversationParticipants.Add(new ConversationParticipant()
            {
                UserId = groupMember.Id,
                Role = GroupRole.GroupMember
            });
        }


        var newGroup = new Conversation()
        {
            Id = Guid.NewGuid(),
            ConversationType = ConvoType.Group,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = request.RequesterId.ToString(),
            Participants = conversationParticipants,
            ImageUrl = request.ImageUrl,
            Description = request.Description,
        };
        await conversationRepo.AddAsync(newGroup);
        await unitOfWork.SaveChangesAsync();
        
        var mappedGroup = mapper.Map<ConversationDto>(newGroup);
        if (mappedGroup == null)
        {
            return Error.BadRequest("Conversation.Invalid", "Invalid Conversation");
        }


        foreach (var participant in checkedUser)
        {
            await realtimeNotifier.NotifyNewGroupConversationAsync(participant.Id, mappedGroup);
        }

        return Result<ConversationDto>.Ok(mappedGroup);
    }
}