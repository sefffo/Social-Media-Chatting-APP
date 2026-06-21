using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;

namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;

public class BlockedUserItemDto
{
    public PublicUserProfileDto User { get; set; }
    public DateTime BlockedAt { get; set; }
}