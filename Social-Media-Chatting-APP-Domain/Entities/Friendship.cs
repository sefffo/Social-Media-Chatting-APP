namespace Social_Media_Chatting_APP_Domain.Entities;



public enum FriendshipStatus : byte
{
    Pending=1,
    Accepted=2,
    Rejected=3,
    Blocked=4
}

public class Friendship : BaseEntity<Guid>
{
    
    public Guid RequestId { set; get; }
    public Guid AddresseeId   { set; get; }
    
    public FriendshipStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { set; get; }
    public Guid? BlockedById { set; get; }
    
}
