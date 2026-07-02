using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;

namespace Social_Media_Chatting_APP_Service.Common;

public class RealTimeNotifierService : IRealtimeNotifier
{
    public Task BroadcastNewMessage(Guid conversationId, MessageDto message)
    {
        return Task.CompletedTask;
    }

    public Task BroadcastReadReceipt(Guid conversationId, Guid readerId, List<Guid> messageId)
    {
        return Task.CompletedTask;
    }

    public Task AddToConversationGroup(Guid conversationId, string connectionId)
    {
        return Task.CompletedTask;
    }

    public Task NotifyNewConversationAsync(string targetUserId, ConversationDto conversation)
    {
        // TODO: implement when SignalR hub is wired up
        // await _hubContext.Clients.User(targetUserId).SendAsync("NewConversation", conversation);
        return Task.CompletedTask;
    }

    public Task NotifyNewGroupConversationAsync(string targetUserIds, ConversationDto conversation)
    {
        // TODO: implement when SignalR hub is wired up
        // await _hubContext.Clients.User(targetUserId).SendAsync("NewConversation", conversation);
        return Task.CompletedTask;
    }
}