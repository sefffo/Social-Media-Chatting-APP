using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_ServiceAbstraction;

namespace Social_Media_Chatting_APP_Presentation.Hubs;

[Authorize]
public class ChatHub(
    IUnitOfWork unitOfWork,
    IRealtimeNotifier realtimeNotifier
) : Hub
{
    public override async Task OnConnectedAsync()
    {
        //check for the user Token 
        var user = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        //un Auth Abort 
        if (user is null)
        {
            //abort the connection 
            Context.Abort();
            return;
        }

        //get his conversation ids and add to the group 
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();
        var conversations = await conversationRepo.FindAllAsync(c => c.Participants.Any(p => p.UserId == user));
        var conversationIdsList = conversations.Select(c => c.Id).ToList();

        foreach (var id in conversationIdsList)
        {
            await realtimeNotifier.AddToConversationGroup(id, Context.ConnectionId);
        }


        await base.OnConnectedAsync();
    }

    //for the disconnect
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    async Task<bool> CheckIfUserIsParticipantOfConversation(Guid User, Guid conversationId)
    {
        var user = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();
        var convo = await conversationRepo.FindAsync(c => c.Id == conversationId &&
                                                          c.Participants.Any(p => p.UserId == user));
        var convoId = convo?.Id;

        return convo is not null;
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var user = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var check = await CheckIfUserIsParticipantOfConversation(Guid.Parse(user), conversationId);
        //check if user is participant of the convo 
        if (check)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }
        else
        {
            throw new Exception("User is not a participant of the conversation");
        }
    }

    public async Task SendTypingIndicator(Guid conversationId, bool isTyping)
    {
        var user = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        //check if he is participant 

        var isParticipant = await CheckIfUserIsParticipantOfConversation(Guid.Parse(user), conversationId);

        if (isParticipant)
        {
            await Clients.OthersInGroup(conversationId.ToString()).SendAsync("ReceiveTypingIndicator", user, isTyping);
        }


        //await base.OnConnectedAsync(); ==> not needed as it's a regular hub method 
    }
}