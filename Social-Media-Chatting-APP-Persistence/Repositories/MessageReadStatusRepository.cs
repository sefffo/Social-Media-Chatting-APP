using Microsoft.EntityFrameworkCore;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Persistence.DbContext;

namespace Social_Media_Chatting_APP_Persistence.Repositories;

public class MessageReadStatusRepository(
    Social_Media_Chatting_APP_DbContext context
) : IMessageReadStatusRepository
{
    public async Task<List<Guid>> GetAlreadyReadMessageIdsAsync(Guid readerId,
        IEnumerable<Guid> candidateIds)
    {
        var readerIdString = readerId.ToString();
        var alreadyReadIds = await context.MessageReadStatuses
            .Where(rs => rs.UserId == readerIdString &&
                         candidateIds.Contains(rs.MessageId))
            .Select(rs => rs.MessageId)
            .ToListAsync();
        
        return alreadyReadIds;
    }

    public async Task AddReadStatusesAsync(Guid readerId, IEnumerable<Guid> messageIds, DateTime readAt)
    {
        var readerIdString = readerId.ToString();

        var entities = messageIds.Select(id => new MessageReadStatus
        {
            MessageId = id,
            UserId = readerIdString,
            ReadAt = readAt
        });

        await context.MessageReadStatuses.AddRangeAsync(entities);
        // do NOT call SaveChanges here; the handler will call unitOfWork.SaveChangesAsync()
    }
}