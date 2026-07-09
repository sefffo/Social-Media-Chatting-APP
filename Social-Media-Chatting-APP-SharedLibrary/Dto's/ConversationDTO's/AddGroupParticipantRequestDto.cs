namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;

public class AddGroupParticipantRequestDto
{
    public List<Guid> NewParticipantIds { get; set; } = new();
}