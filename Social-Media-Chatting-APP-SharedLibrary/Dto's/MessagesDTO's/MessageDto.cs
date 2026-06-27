using Microsoft.VisualBasic;
using Social_Media_Chatting_APP_Domain.Entities.Enums;

namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;

public class MessageDto
{ 
    
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public DateTime SentAt { get; set; }
    
    public MessageContentType ContentType { get; set; }
    public string? TextContent { get; set; }
    public string? MediaUrl { get; set; }
    public string? FileName { get; set; }
    
    public SenderDto Sender { get; set; }
    
    public bool IsReply { get; set; }
    public Guid? ReplyTo { get; set; }
    public bool IsRead { get; set; }
    public int ReadByCount { get; set; }
    public bool IsDeleted { get; set; } 
    
    
}