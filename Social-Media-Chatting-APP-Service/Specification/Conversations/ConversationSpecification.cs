using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Specifications;

namespace Social_Media_Chatting_APP_Service.Specification.Conversations;
// fetches conversation WITH its participants for security checks
public class ConversationSpecification : BaseSpecification<Conversation>
{
    public ConversationSpecification(Guid ConversationId) : base(c => c.Id == ConversationId)
    {
        AddIncludes(c => c.Participants);
    }
}