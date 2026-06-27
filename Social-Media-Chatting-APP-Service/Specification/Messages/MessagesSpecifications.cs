using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Specifications;

namespace Social_Media_Chatting_APP_Service.Specification.Messages;

public class MessagesSpecifications : BaseSpecification<Message>
{
    public MessagesSpecifications(Guid conversationId, int pageIndex, int pageSize) : base(m =>
        m.ConversationId == conversationId && !m.IsDeleted)
    {
        AddIncludes(m => m.Sender);
        AddIncludes(m => m.ReadStatuses);
        ApplyOrderByDescending(m => m.SentAt);
        ApplyPagination(pageSize, pageIndex);
    }
}