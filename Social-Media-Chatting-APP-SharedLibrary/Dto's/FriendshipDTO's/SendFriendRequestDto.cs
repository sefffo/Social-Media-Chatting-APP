namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;

public class SendFriendRequestDto
{
    public Guid AddresseeId { get; set; } //based on user Token To avoid SQL Injection
}