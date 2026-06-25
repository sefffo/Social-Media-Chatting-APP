using Social_Media_Chatting_APP_Domain.Entities.Enums;

namespace Social_Media_Chatting_APP_Domain.Entities;

public class Conversation : BaseEntity<Guid>
{
    public ConvoType ConversationType { get; set; }
    public string? Name { set; get; } //null for DM but needed for group
    public DateTime CreatedAt { get; set; }

    public string CreatedByUserId { get; set; }

    //nav prop
    public AppUser CreatedByUser { set; get; }

    public string LastMessageId { get; set; }

    //nav prop 
    public Message? LastMessage { set; get; } //nullable bec if that's a new conversation

    public DateTime? LastMessageAt { set; get; }
    public ICollection<ConversationParticipant> Participants { set; get; } = [];

    public ICollection<Message> Messages { set; get; } = [];

}