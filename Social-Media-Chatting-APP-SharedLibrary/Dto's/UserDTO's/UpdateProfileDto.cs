namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;

public class UpdateProfileDto
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public bool? ShowOnlineStatus { get; set; }
    public bool? ShowLastSeen { get; set; }
    public bool? AllowMessageFromStrangers { get; set; }
}