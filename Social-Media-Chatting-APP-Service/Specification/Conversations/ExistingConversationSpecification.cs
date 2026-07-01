using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_Domain.Specifications;

namespace Social_Media_Chatting_APP_Service.Specification.Conversations;

public class ExistingConversationSpecification : BaseSpecification<Conversation>
{
    public ExistingConversationSpecification(Guid userA, Guid userB) : base(c =>
        c.ConversationType == ConvoType.DirectMessage && c.Participants.Any(p => p.UserId == userA.ToString()) &&
        c.Participants.Any(p => p.UserId == userB.ToString()))
    {
        AddIncludes(c => c.Participants);
    }
}