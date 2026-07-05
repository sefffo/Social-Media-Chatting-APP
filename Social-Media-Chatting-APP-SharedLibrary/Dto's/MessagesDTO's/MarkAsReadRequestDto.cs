namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;

public class MarkAsReadRequestDto
{
    public Guid ConversationId { get; set; }
    public Guid UpToMessageId { get; set; }
}