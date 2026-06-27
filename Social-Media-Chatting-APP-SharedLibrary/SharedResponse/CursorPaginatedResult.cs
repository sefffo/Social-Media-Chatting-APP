namespace Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

public class CursorPaginatedResult<T> where T : class
{
    // The actual messages for this batch
    public IEnumerable<T> Data { get; }

    // Are there older messages to load when user scrolls further up?
    public bool HasMore { get; }

    // The SentAt of the OLDEST message in this batch
    // The client sends this back as the "before" cursor on the next scroll
    // Null when this batch is empty or there are no more messages
    public DateTime? NextCursor { get; }

    private CursorPaginatedResult(IEnumerable<T> data, bool hasMore, DateTime? nextCursor)
    {
        Data = data;
        HasMore = hasMore;
        NextCursor = nextCursor;
    }

    public static CursorPaginatedResult<T> Create(IEnumerable<T> data, int pageSize, Func<T, DateTime> getCursor)
    {
        var list = data.ToList();

        // If we got back MORE than pageSize, that means there are more records
        // We fetch pageSize + 1 from DB, use the extra one only to determine HasMore
        var hasMore = list.Count > pageSize;

        // Trim the extra item we fetched — don't send it to the client
        if (hasMore) list = list.Take(pageSize).ToList();

        // The cursor is the SentAt of the oldest message in the batch (last item)
        // because messages are ordered newest first
        var nextCursor = hasMore ? getCursor(list.Last()) : (DateTime?)null;

        return new CursorPaginatedResult<T>(list, hasMore, nextCursor);
    }
}