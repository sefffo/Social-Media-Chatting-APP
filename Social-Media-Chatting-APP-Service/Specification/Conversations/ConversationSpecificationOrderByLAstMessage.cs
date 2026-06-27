using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Specifications;

namespace Social_Media_Chatting_APP_Service.Specification.Conversations;

public class ConversationSpecificationOrderByLAstMessage : BaseSpecification<Conversation>
{
    // Fetches all conversations for a user — ordered by last activity
    public ConversationSpecificationOrderByLAstMessage(string userId)
        : base(c => c.Participants.Any(p => p.UserId == userId))
    {
        AddIncludes(c => c.Participants);
        ApplyOrderByDescending(c => c.LastMessageAt);
    }
    
    
}