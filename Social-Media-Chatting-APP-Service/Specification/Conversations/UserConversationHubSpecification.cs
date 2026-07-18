using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Specifications;

namespace Social_Media_Chatting_APP_Service.Specification.Conversations;

public class UserConversationHubSpecification : BaseSpecification<Conversation>
{
    public UserConversationHubSpecification(Guid userId) : base(c =>
        c.Participants.Any(p => p.UserId == userId.ToString()))
    {
        AddIncludes(c => c.Participants);
    }
}