using Microsoft.AspNetCore.Http;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.CloudinaryDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Enums;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_ServiceAbstraction;

public interface IUploadService
{
    Task<Result<CloudinaryUploadResultDto>> UploadFileAsync(
        IFormFile file,
        UploadPurpose purpose,
        Guid uploaderUserId,
        Guid? conversationId = null,
        FileResourceType resourceType = FileResourceType.Auto);

    public Task<Result> DeleteFileAsync(string publicId, FileResourceType resourceType = FileResourceType.Auto);
}