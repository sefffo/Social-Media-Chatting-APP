using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;

namespace Social_Media_Chatting_APP_Service.Common;

public class RealTimeNotifierService : IRealtimeNotifier
{
    private IRealtimeNotifier _realtimeNotifierImplementation;

    public Task BroadcastNewMessage(Guid conversationId, MessageDto message)
    {
        return Task.CompletedTask;
    }

    public Task BroadcastReadReceipt(Guid conversationId, Guid readerId, List<Guid> messageId)
    {
        return Task.CompletedTask;
    }

    public Task AddToConversationGroup(List<Guid> conversationId, string connectionId)
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

    public Task NotifyGroupUpdatedAsync(string participantUserId, ConversationDto mappedGroup)
    {
        //TODO: implement when SignalR hub is wired up
        return Task.CompletedTask;
    }

    public Task NotifyRemovedFromGroupAsync(string targetUserId, Guid requestConversationId)
    {
        //TODO: implement when SignalR hub is wired up
        return Task.CompletedTask;
    }

    public Task NotifyRoleChangedAsync(string targetUserId, Guid requestConversationId, GroupRole requestNewRole)
    {
        //TODO: implement when SignalR hub is wired up
        return Task.CompletedTask;
    }

    public Task NotifyGroupDeletedAsync(string userId, Guid requestConversationId)
    {
        //TODO: implement when SignalR hub is wired up
        return Task.CompletedTask;
    }
}