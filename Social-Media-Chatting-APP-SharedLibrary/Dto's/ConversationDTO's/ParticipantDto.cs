namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;

public class ParticipantDto
{
    public string UserId { get; set; }
    public string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsOnline { get; set; }  // populated from PresenceTracker later
}