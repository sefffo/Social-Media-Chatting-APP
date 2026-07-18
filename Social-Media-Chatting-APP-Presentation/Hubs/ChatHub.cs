using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Specification.Conversations;
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
        var user = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user is null)
        {
            Context.Abort();
            return;
        }

        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();
        var conversations = await conversationRepo.FindAllAsync(new UserConversationHubSpecification(Guid.Parse(user)));

        foreach (var id in conversations.Select(c => c.Id))
            await realtimeNotifier.AddToConversationGroup(id, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> CheckIfUserIsParticipantOfConversation(Guid conversationId)
    {
        var user = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var conversationRepo = unitOfWork.GetRepository<Conversation, Guid>();
        var convo = await conversationRepo.FindAsync(
            new ConversationMembershipSpecification(conversationId, user));
        return convo is not null;
    }

    /// <summary>
    /// Called by the client when they open a conversation.
    /// Adds the connection to the SignalR group so they receive live messages.
    /// </summary>
    public async Task JoinConversation(Guid conversationId)
    {
        var isParticipant = await CheckIfUserIsParticipantOfConversation(conversationId);
        if (isParticipant)
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        else
            throw new HubException("User is not a participant of the conversation");
    }

    /// <summary>
    /// Called by the client (or server-side after removal) to leave a conversation group.
    /// The client stops receiving BroadcastNewMessage events for this conversation.
    /// This is the client-callable counterpart to RemoveFromConversationGroup.
    /// </summary>
    public async Task LeaveConversation(Guid conversationId)
    {
        var isParticipant = await CheckIfUserIsParticipantOfConversation(conversationId);
        if (!isParticipant)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
        else
            throw new HubException("You are still a participant of this conversation");
    }

    /// <summary>
    /// Broadcasts a typing indicator to all other members in the conversation.
    /// The client sends isTyping=true when typing starts and false when they stop.
    /// </summary>
    public async Task SendTypingIndicator(Guid conversationId, bool isTyping)
    {
        var user = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isParticipant = await CheckIfUserIsParticipantOfConversation(conversationId);

        if (isParticipant)
            await Clients.OthersInGroup(conversationId.ToString())
                .SendAsync("ReceiveTypingIndicator", user, conversationId, isTyping);
    }
}