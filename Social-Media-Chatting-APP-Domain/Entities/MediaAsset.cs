using Social_Media_Chatting_APP_Domain.Entities.Enums;

namespace Social_Media_Chatting_APP_Domain.Entities;

public class MediaAsset : BaseEntity<Guid>
{
    public string UploaderId { get; set; }

    //nav prop 
    public AppUser Uploader { set; get; }

    public string Url { get; set; }
    public ResourceType ResourceType { get; set; }

    public string OriginalFileName { set; get; }

    public string PublicId { set; get; }

    public string FolderName { set; get; }

    public string Format { set; get; }

    public long Size { set; get; }

    public DateTime CreatedAt { set; get; }
    public DateTime? DeletedAt { set; get; }
    public bool IsDeleted { set; get; }

    public Guid? MessageId { set; get; }

    //nav prop
    public Message? Message { set; get; }

    public Guid? ConversationId { set; get; }

    //nav prop
    public Conversation? Conversation { set; get; }
}