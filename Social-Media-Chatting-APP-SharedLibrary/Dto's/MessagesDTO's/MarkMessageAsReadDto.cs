namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;

public class MarkMessageAsReadDto
{
    Guid ConversationId { get; set; }
    Guid UpToMessageId { get; set; }
    List<Guid> NewlyMessageIds { get; set; }
    DateTime? ReadAt { get; set; }   
}