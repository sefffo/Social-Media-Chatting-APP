using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Conversations.Group;
using Social_Media_Chatting_APP_Service.Features.Conversations.Group.Helpers;
using Social_Media_Chatting_APP_Service.Features.Friendship.Helpers;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Commands.Group;

public class AddGroupParticipantCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRealtimeNotifier realtimeNotifier,
    UserManager<AppUser> userManager) : IRequestHandler<AddGroupParticipantCommand, Result<ConversationDto>>
{
    public async Task<Result<ConversationDto>> Handle(AddGroupParticipantCommand request,
        CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();

        var conversation = await conversationRepo.GetByIdAsync(request.ConversationId);

        if (conversation is null)
        {
            return Error.NotFound("Conversation.NotFound", "No conversation with that Id was found");
        }


        if (conversation.ConversationType != ConvoType.Group)
            return Error.BadRequest("Conversation.NotGroup", "This operation only applies to groups");


        var adminCheck = GroupPermissionHelper.IsGroupAdmin(conversation, request.RequesterId.ToString());

        if (adminCheck)
        {
            return Error.Forbidden("Conversation.NotAdmin", "Only group admins can add members to the group");
        }

        var friendshipRepo = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();
        var newParticipantIds = request.NewParticipantId
            .Where(id => id != request.RequesterId)
            .Distinct()
            .ToList();
        if (newParticipantIds.Count == 0)
            return Error.BadRequest("Conversation.NoParticipants", "No valid participants to add");
        var newUsers = new List<AppUser>();
        foreach (var newParticipantId in newParticipantIds)
        {
            var participant = GroupPermissionHelper.IsParticipant(conversation, newParticipantId.ToString());
            if (participant)
            {
                return Error.BadRequest("Conversation.AlreadyMember",
                    $"User {newParticipantId} is already a member of this group");
            }
            //if he is not in the group, so we check first, and then we add 

            var user = await userManager.FindByIdAsync(newParticipantId.ToString());
            if (user is null)
            {
                return Error.NotFound("User.NotFound", $"User {newParticipantId} not found");
            }

            var friendshipCheck =
                await FriendshipQueryHelper.GetAsync(friendshipRepo, request.RequesterId, newParticipantId);
            if (friendshipCheck is null || friendshipCheck.Status != FriendshipStatus.Accepted)
                return Error.Forbidden("Conversation.NotFriends",
                    $"User {user.UserName} is not your friend");

            newUsers.Add(user);
        }

        foreach (var user in newUsers)
        {
            conversation.Participants.Add(new ConversationParticipant()
            {
                UserId = user.Id,
                Role = GroupRole.GroupMember
            });
        }

        conversationRepo.Update(conversation);
        await unitOfWork.SaveChangesAsync();

        var mappedGroup = mapper.Map<ConversationDto>(conversation);

        foreach (var user in newUsers)
        {
            await realtimeNotifier.NotifyNewGroupConversationAsync(user.Id, mappedGroup);
        }

        foreach (var participant in conversation.Participants.Where(p =>
                     !newUsers.Select(u => u.Id).Contains(p.UserId)))
        {
            await realtimeNotifier.NotifyGroupUpdatedAsync(participant.UserId, mappedGroup);
        }

        return Result<ConversationDto>.Ok(mappedGroup);
    }
}