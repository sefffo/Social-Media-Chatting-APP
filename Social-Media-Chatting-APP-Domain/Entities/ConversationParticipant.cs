namespace Social_Media_Chatting_APP_Domain.Entities;

public class ConversationParticipant  // no BaseEntity
{
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;
    
    public string UserId { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    
    public DateTime JoinedAt { get; set; }
    public bool IsAdmin { get; set; }
}
