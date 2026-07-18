using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Social_Media_Chatting_APP_Presentation.Hubs;

///// <summary>
////User A creates a DM with User B
////        ↓
////CreateDmConversationCommandHandler runs
////        ↓
////Saves conversation to DB
////        ↓
////Calls: realtimeNotifier.NotifyNewConversationAsync("userB-id", conversationDto)
////        ↓
////SignalRRealtimeNotifier receives this call
////        ↓
////Uses IHubContext<NotificationHub>
////        ↓
////Calls: Clients.User("userB-id").SendAsync("NewConversation", conversationDto)
////        ↓
////SignalR looks up: "which connections belong to userB-id?"
////        ↓
////IUserIdProvider already mapped userB's connection → "userB-id" when they connected 
////        ↓
////SignalR delivers "NewConversation" event + conversationDto to userB's client
////        ↓
////User B's UI receives it and shows the new DM in their chat list
/////</summary
[Authorize]
public class NotificationHub : Hub
{
   
}