using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;

namespace Social_Media_Chatting_APP_Service.Common;

public class RealTimeNotifierService : IRealtimeNotifier
{
    public Task BroadcastNewMessage(Guid conversationId, MessageDto message)
    {
        throw new NotImplementedException();
    }

    public Task BroadcastReadReceipt(Guid conversationId, Guid readerId, List<Guid> messageId)
    {
        throw new NotImplementedException();
    }

    public Task AddToConversationGroup(Guid conversationId, string connectionId)
    {
        throw new NotImplementedException();
    }
}