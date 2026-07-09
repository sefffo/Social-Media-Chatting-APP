using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Group;

public class UpdateGroupInfoCommandHandler(
    UserManager<AppUser> userManager,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRealtimeNotifier realtimeNotifier
) : IRequestHandler<UpdateGroupInfoCommand, Result<ConversationDto>>
{
    public async Task<Result<ConversationDto>> Handle(UpdateGroupInfoCommand request,
        CancellationToken cancellationToken)
    {
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();
        //check if the user exists and if he is an admin 
        var userChecked = await userManager.Users.FirstOrDefaultAsync(a => a.Id == request.RequesterId.ToString());
        if (userChecked is null)
        {
            return Error.NotFound("member.NotFound", "User not found");
        }

        var conversation = await conversationRepo.GetByIdAsync(request.ConversationId);
        if (conversation is null)
        {
            return Error.NotFound(" Conversation.NotFound ", "Conversation not found");
        }
        if (conversation.ConversationType != ConvoType.Group)
            return Error.BadRequest("Conversation.NotGroup", "This operation only applies to groups");

        var adminCheck =
            conversation.Participants.Any(p => p.UserId == request.RequesterId.ToString() && p.Role == GroupRole.GroupAdmin);

        if (!adminCheck)
        {
            return Error.Forbidden("Conversation.NotAdmin", "Only group admins can update group info");
        }
        if (!string.IsNullOrWhiteSpace(request.Name))
            conversation.Name = request.Name;

        if (request.Description is not null)
            conversation.Description = request.Description;

        if (request.ImageUrl is not null)
            conversation.ImageUrl = request.ImageUrl;

        conversationRepo.Update(conversation);
        await unitOfWork.SaveChangesAsync();

        var mappedGroup = mapper.Map<ConversationDto>(conversation);
        
        foreach (var participant in conversation.Participants)
        {
            await realtimeNotifier.NotifyGroupUpdatedAsync(participant.UserId, mappedGroup);
        }

        return Result<ConversationDto>.Ok(mappedGroup);
    }
}