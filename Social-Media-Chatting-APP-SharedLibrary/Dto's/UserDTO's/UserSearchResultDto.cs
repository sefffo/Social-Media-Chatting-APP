namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;

public class UserSearchResultDto
{
    public string UserName { get; set; }
    public string DisplayName { get; set; }
    public string ProfilePictureUrl { get; set; }
    public bool IsOnline { get; set; }//show only for friends 
    public DateTime LastSeen { get; set; }//show only for friends 
    public DateTime CreatedAt { get; set; }//show only for friends 
}