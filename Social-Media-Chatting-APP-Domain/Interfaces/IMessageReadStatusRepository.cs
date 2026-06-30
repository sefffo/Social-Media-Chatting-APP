namespace Social_Media_Chatting_APP_Domain.Interfaces;

public interface IMessageReadStatusRepository
{
    Task<List<Guid>> GetAlreadyReadMessageIdsAsync( Guid readerId, IEnumerable<Guid> candidateIds);
    Task AddReadStatusesAsync(Guid readerId, IEnumerable<Guid> messageIds, DateTime readAt);
}