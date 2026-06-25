using Social_Media_Chatting_APP_Domain.Entities.Enums;

namespace Social_Media_Chatting_APP_Domain.Entities;

public class Message : BaseEntity<Guid>
{
    public Guid ConversationId { get; set; }
    //nav prop 
    public Conversation Conversation { set; get; }
    
    public string SenderId { set; get; }
    //nav prop
    public AppUser Sender { set; get; }
    
    public MessageContentType Content { set; get; }
    public DateTime SentAt { set; get;}
    //Cloudinary and upload 
    public string? MediaUrl { set; get; }
    public string? MediaPublicId { set; get; }
    public string? FileName { set; get; }
    public  bool IsDeleted { set; get; }
    
    //nav prop for Read status 
    public ICollection<MessageReadStatus> ReadStatuses { set; get; } = [];

}