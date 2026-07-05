namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;

public class CreateGroupConversationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public List<Guid> ParticipantsIds { get; set; } = [];
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
}