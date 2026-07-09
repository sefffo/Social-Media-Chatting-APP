using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Social_Media_Chatting_APP_Domain.Entities.Enums;
using Social_Media_Chatting_APP_Service.Common.Upload;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.CloudinaryDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Enums;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController(
    IServiceScopeFactory serviceScopeFactory,
    BackgroundUploadQueue uploadQueue
) : ApiBaseController
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CloudinaryUploadResultDto>> UploadFile(
        [FromForm] IFormFile file,
        [FromForm] UploadPurpose purpose,
        [FromForm] Guid? conversationId,
        [FromForm] FileResourceType resourceType = FileResourceType.Auto)
    {
        var uploaderUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();
        var fileName = file.FileName;
        var contentType = file.ContentType;

        // This is the signal the controller will wait on.
        var completionSource = new TaskCompletionSource<Result<CloudinaryUploadResultDto>>();

        await uploadQueue.EnqueueAsync(async token =>
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var scopedUploadService = scope.ServiceProvider.GetRequiredService<IUploadService>();

                using var stream = new MemoryStream(fileBytes);
                var reconstructedFile = new FormFile(stream, 0, stream.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };

                var result = await scopedUploadService.UploadFileAsync(
                    reconstructedFile, purpose, uploaderUserId, conversationId, resourceType);

                completionSource.SetResult(result);
            }
            catch (Exception ex)
            {
                completionSource.SetException(ex);
            }
        });

        // Controller blocks here, but the HTTP request stays open with a spinner
        // on the client side, exactly matching your intended UX.
        var uploadResult = await completionSource.Task;

        return HandleResult(uploadResult);
    }
}