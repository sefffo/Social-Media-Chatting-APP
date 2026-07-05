using Social_Media_Chatting_APP_Domain.Entities.Enums;

namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;

public class SendMessageRequestDto
{
    public Guid ConversationId { get; set; }
    public MessageContentType ContentType { get; set; }
    public string? TextContent { get; set; }
    public string? MediaUrl { get; set; }
    public string? FileName { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public string? MediaPublicId { get; set; }
}
