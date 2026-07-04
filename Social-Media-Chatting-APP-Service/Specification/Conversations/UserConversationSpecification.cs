using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Specifications;

namespace Social_Media_Chatting_APP_Service.Specification.Conversations;

public class UserConversationSpecification : BaseSpecification<Conversation>
{
    public UserConversationSpecification(Guid requesterId, DateTime? before, int pageSize) :
        base(c => c.Participants.Any(p => p.UserId == requesterId.ToString()) &&
                  (before == null || c.LastMessageAt < before))
    {
        AddIncludes(c => c.Participants);
        AddIncludes("Participants.User"); // ThenInclude via string — loads AppUser nav
        
        AddIncludes(m => m.LastMessage);
        // Include Messages with their ReadStatuses so we can count unread
        AddIncludes("Messages.ReadStatuses");
        //AddIncludes(m => m.LastMessageAt);
        ApplyOrderByDescending(m => m.LastMessageAt);
        ApplyTake(pageSize + 1);
    }
}