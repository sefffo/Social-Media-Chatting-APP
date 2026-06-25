namespace Social_Media_Chatting_APP_Domain.Entities;

public class MessageReadStatus  // no BaseEntity
{
    public Guid MessageId { get; set; }
    public Message Message { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public AppUser User { get; set; } = null!;

    public DateTime ReadAt { get; set; }
}