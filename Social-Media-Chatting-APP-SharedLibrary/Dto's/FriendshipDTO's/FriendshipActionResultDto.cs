namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;

public class FriendshipActionResultDto
{
    public Guid FriendshipId { get; set; }
    public Guid RequesterId  { get; set; }
    public Guid AddresseeId  { get; set; }
    public string Status     { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
