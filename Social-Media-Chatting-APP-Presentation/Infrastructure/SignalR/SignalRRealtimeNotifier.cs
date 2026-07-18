using Microsoft.AspNetCore.SignalR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Presentation.Hubs;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;

namespace Social_Media_Chatting_APP_Presentation.Infrastructure.SignalR;

public class SignalRRealtimeNotifier(
    IHubContext<ChatHub> chatHubContext,
    IHubContext<NotificationHub> notificationHubContext
) : IRealtimeNotifier
{
    public async Task BroadcastNewMessage(Guid conversationId, MessageDto message)
        => await chatHubContext.Clients.Group(conversationId.ToString()).SendAsync("MessageReceived", message);

    public async Task BroadcastReadReceipt(Guid conversationId, Guid readerId, List<Guid> messageId)
        => await chatHubContext.Clients.Group(conversationId.ToString()).SendAsync("ReadReceipt", readerId, messageId);

    public async Task AddToConversationGroup(Guid conversationId, string connectionId)
        => await chatHubContext.Groups.AddToGroupAsync(connectionId, conversationId.ToString());


    public async Task NotifyNewConversationAsync(string targetUserId, ConversationDto conversation)
        => await notificationHubContext.Clients.User(targetUserId).SendAsync("NewConversation", conversation);

    public async Task NotifyNewGroupConversationAsync(string targetUserIds, ConversationDto conversation)
        => await notificationHubContext.Clients.Users(targetUserIds).SendAsync("NewGroupConversation", conversation);

    public async Task NotifyGroupUpdatedAsync(string participantUserId, ConversationDto mappedGroup)
        => await notificationHubContext.Clients.User(participantUserId).SendAsync("GroupUpdated", mappedGroup);

    public async Task NotifyRemovedFromGroupAsync(string targetUserId, Guid requestConversationId)
        => await notificationHubContext.Clients.User(targetUserId).SendAsync("RemovedFromGroup", requestConversationId);

    public async Task NotifyRoleChangedAsync(string targetUserId, Guid requestConversationId, GroupRole requestNewRole)
        => await notificationHubContext.Clients.User(targetUserId)
            .SendAsync("RoleChanged", requestConversationId, requestNewRole);

    public async Task NotifyGroupDeletedAsync(string userId, Guid requestConversationId)
        => await notificationHubContext.Clients.User(userId).SendAsync("GroupDeleted", requestConversationId);
}