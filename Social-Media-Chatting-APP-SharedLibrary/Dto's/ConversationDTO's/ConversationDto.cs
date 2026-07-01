using Social_Media_Chatting_APP_Domain.Entities.Enums;

namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;

public class ConversationDto
{
    public Guid Id { get; set; }
    
    //optional as if group convo if exists we gonna need that 
    public string? ImageUrl { get; set; }
    public string? Name { get; set; }
    public ConvoType ConversationType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }

    public int UnreadCount { get; set; }

    public List<ParticipantDto> OtherParticipant { get; set; }
}