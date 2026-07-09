namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.CloudinaryDTO_s;

public class CloudinaryUploadResultDto
{
    public Guid MediaAssetId { get; set; }
    public string Url { get; set; }        // the secure HTTPS URL
    public string FileName { get; set; } //to get the file name for sending messages 
    public string PublicId { get; set; }   // needed for deletion later
    public string ResourceType { get; set; } // "image", "video", "raw"
    public long Size { get; set; }         // file size in bytes
    public string Format { get; set; }     // "jpg", "mp4", "pdf", etc.
}