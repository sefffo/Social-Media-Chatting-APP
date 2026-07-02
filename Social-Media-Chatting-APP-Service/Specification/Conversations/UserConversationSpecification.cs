using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Specifications;

namespace Social_Media_Chatting_APP_Service.Specification.Conversations;

public class UserConversationSpecification : BaseSpecification<Conversation>
{
    
    public UserConversationSpecification(Guid requesterId , DateTime? before , int pageSize) : 
        base(c => c.Participants.Any(p=> p.UserId == requesterId.ToString()))
    {
        AddIncludes(c => c.Participants);
        AddIncludes(m=>m.LastMessage);
        AddIncludes(m=>m.LastMessageAt);
        ApplyOrderByDescending(m=>m.LastMessageAt);
        ApplyTake(pageSize+1);
    }
    
    
}