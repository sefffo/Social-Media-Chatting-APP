namespace Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

public class PaginatedResult<T> where T : class
{
    public IEnumerable<T> Data { get; }

    // Which page the client requested — starts at 1, not 0
    public int CurrentPage { get; }

    // How many items per page — e.g. 20 messages per page
    public int PageSize { get; }

    // The TOTAL number of records in the database matching the filter
    // This is NOT just the count of Data — it's the full count before paging
    // e.g. there are 135 messages total, but Data only contains 20
    public int TotalCount { get; }

    // Calculated — how many pages exist in total
    // Math.Ceiling ensures we round UP — 135 messages / 20 per page = 6.75 → 7 pages
    // Without Ceiling we'd lose the last partial page
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    // Calculated — is there a page after this one?
    // Used by the client to enable/disable the "Next" button
    public bool HasNextPage => CurrentPage < TotalPages;

    // Calculated — is there a page before this one?
    // Used by the client to enable/disable the "Previous" button
    public bool HasPreviousPage => CurrentPage > 1;

    // Private constructor — forces usage of the static Create method below
    // This is intentional: Create reads more clearly in handlers than new()
    private PaginatedResult(IEnumerable<T> data, int currentPage, int pageSize, int totalCount)
    {
        Data = data;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    // Static factory method — the clean way to build this from a handler
    // Usage: PaginatedResult<MessageDto>.Create(messages, pageIndex, pageSize, totalCount)
    // The handler passes in the four raw values, everything else is calculated automatically
    public static PaginatedResult<T> Create(IEnumerable<T> data, int currentPage, int pageSize, int totalCount)
        => new PaginatedResult<T>(data, currentPage, pageSize, totalCount);
}