using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;

namespace Social_Media_Chatting_APP_ServiceAbstraction;

/// <summary>
///"Somewhere in this system, something exists that can broadcast real-time events.
/// I don't know if it's SignalR, WebSockets, or Firebase — and I don't care.
/// I just call these methods and trust it works."
/// </summary>
public interface IRealtimeNotifier
{
    /// <summary>
    /// Broadcasts a new message to everyone in the conversation group.
    /// </summary>
    public Task BroadcastNewMessage(Guid conversationId, MessageDto message);

    /// <summary>
    /// Broadcasts read receipts so senders see their ticks turn blue.
    /// Delivers: which messages were read, and by whom.
    /// </summary>
    public Task BroadcastReadReceipt(Guid conversationId, Guid readerId, List<Guid> messageId);

    /// <summary>
    /// Adds a specific SignalR connection to a conversation group.
    /// Called from ChatHub.OnConnectedAsync and ChatHub.JoinConversation — not from handlers.
    /// </summary>
    public Task AddToConversationGroup(Guid conversationId, string connectionId);

    /// <summary>
    /// Notifies a specific user that a new DM conversation was created with them.
    /// </summary>
    public Task NotifyNewConversationAsync(string targetUserId, ConversationDto conversation);

    /// <summary>
    /// Notifies a specific user they were added to a new group conversation.
    /// Called once per new member in a loop.
    /// </summary>
    public Task NotifyNewGroupConversationAsync(string targetUserId, ConversationDto conversation);

    public Task NotifyGroupUpdatedAsync(string participantUserId, ConversationDto mappedGroup);
    Task NotifyRemovedFromGroupAsync(string targetUserId, Guid requestConversationId);
    Task NotifyRoleChangedAsync(string targetUserId, Guid requestConversationId, GroupRole requestNewRole);
    Task NotifyGroupDeletedAsync(string userId, Guid requestConversationId);
}