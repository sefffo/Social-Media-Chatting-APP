namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;

public class ConversationDto
{
    public Guid Id { get; set; }
    public DateTime? LastMessageAt { get; set; }    
    public string? LastMessagePreview { get; set; }
    
    public int UnreadCount { get; set; }
    
    public ParticipantDto OtherParticipant { get; set; }

}