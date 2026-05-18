using Microsoft.AspNetCore.Http;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.CloudinaryDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Enums;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Common.Upload;

public class UploadService : IUploadService
{
    public Task<Result<CloudinaryUploadResultDto>> UploadFileAsync(IFormFile file, string Folder, FileResourceType FileResourceType = FileResourceType.Auto)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteFileAsync(string publicId)
    {
        throw new NotImplementedException();
    }
}