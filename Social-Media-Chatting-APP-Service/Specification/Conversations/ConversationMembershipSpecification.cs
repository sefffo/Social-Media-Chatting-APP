using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Specifications;

namespace Social_Media_Chatting_APP_Service.Specification.Conversations;

public class ConversationMembershipSpecification : BaseSpecification<Conversation>
{
    public ConversationMembershipSpecification(Guid conversationId, string userId) : base(c =>
        c.Id == conversationId && c.Participants.Any(p => p.UserId == userId))
    {
        AddIncludes(c => c.Participants);
    }
}