namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;

public class PublicUserProfileDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public bool? IsOnline { get; set; }

    public DateTime? LastSeen { get; set; }
    // future: FollowerCount, PostCount, IsFriend, etc.
}