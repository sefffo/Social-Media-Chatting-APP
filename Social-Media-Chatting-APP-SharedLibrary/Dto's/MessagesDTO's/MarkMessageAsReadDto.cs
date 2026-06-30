namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;

public class MarkMessageAsReadDto
{
    
    public Guid ReaderId { get; set; }    
    public Guid ConversationId { get; set; }
    public Guid UpToMessageId { get; set; }
    public List<Guid> NewlyMessageIds { get; set; }
    public DateTime? ReadAt { get; set; }   
}