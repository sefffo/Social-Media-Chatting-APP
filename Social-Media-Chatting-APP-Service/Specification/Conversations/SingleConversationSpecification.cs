using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Specifications;

namespace Social_Media_Chatting_APP_Service.Specification.Conversations;

public class SingleConversationSpecification : BaseSpecification<Conversation>
{
    public SingleConversationSpecification(Guid conversationId) : base(c =>
        c.Participants.Any(p => p.ConversationId == conversationId)
    )
    {
        AddIncludes(c => c.Participants);
    }
}