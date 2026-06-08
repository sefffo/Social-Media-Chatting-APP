using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;

namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;

public class FriendRequestItemDto
{
    public Guid FriendshipId { get; set; }
    public PublicUserProfileDto User { get; set; } // the OTHER person
    public DateTime SentAt { get; set; }
    public string Direction { get; set; } // "Incoming" or "Outgoing"
}