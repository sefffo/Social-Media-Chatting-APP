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
    /// for the receiver of a message to be able to be notified of the new message 
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task BroadcastNewMessage(Guid conversationId, MessageDto message);

    /// <summary>
    ///Ask yourself: when someone reads messages, who needs to know and what do they need?
    /// The sender of the original messages needs to see their ticks turn blue.
    /// The group is again identified by conversationId.
    /// The receiver needs to know which messages were marked read and by whom. 
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="readerId"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public Task BroadcastReadReceipt(Guid conversationId, Guid readerId, List<Guid> messageId);

    ///  <summary>
    /// This one is different — it's not a broadcast. It's connection management. When a user connects to the hub,
    ///  their connection needs to be added to a SignalR group for each conversation they belong to so future broadcasts reach them.
    /// This gets called from the Hub during OnConnectedAsync, not from a handler.
    ///  But it still belongs on this interface because the Hub will depend on the abstraction, not on the concrete implementation directly.
    /// It needs: connectionId (the unique SignalR connection identifier) and conversationId (the group name to join).
    ///  </summary>
    ///  <param name="conversationId"></param>
    ///  <param name="connectionId"></param>
    ///  <returns></returns>
    public Task AddToConversationGroup(Guid conversationId, string connectionId);
    
    
    /// <summary>
    /// Notifies a specific user that a new conversation was created with them.
    /// The target user's client will receive the conversation and display it in their list.
    /// </summary>
    public Task NotifyNewConversationAsync(string targetUserId, ConversationDto conversation);
    
    public Task NotifyNewGroupConversationAsync(string targetUserIds, ConversationDto conversation);
    public Task NotifyGroupUpdatedAsync(string participantUserId, ConversationDto mappedGroup);
    Task NotifyRemovedFromGroupAsync(string targetUserId, Guid requestConversationId);
    Task NotifyRoleChangedAsync(string targetUserId, Guid requestConversationId, GroupRole requestNewRole);
    Task NotifyGroupDeletedAsync(string userId, Guid requestConversationId);
}