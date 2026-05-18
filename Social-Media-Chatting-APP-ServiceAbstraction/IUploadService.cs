using Microsoft.AspNetCore.Http;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.CloudinaryDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Enums;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_ServiceAbstraction;

public interface IUploadService
{
    public Task<Result<CloudinaryUploadResultDto>> UploadFileAsync(IFormFile file , string Folder  , FileResourceType FileResourceType = FileResourceType.Auto );
    public Task<Result> DeleteFileAsync(string publicId, FileResourceType resourceType = FileResourceType.Auto);
}