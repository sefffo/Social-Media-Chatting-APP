using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_Service.Common.Upload;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.CloudinaryDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Enums;

namespace Social_Media_Chatting_APP_Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController(
    UploadService uploadService
) : ApiBaseController
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CloudinaryUploadResultDto>> UploadFile(
        [FromForm] IFormFile file,
        [FromForm] string folder,
        [FromForm] Guid? conversationId,
        [FromForm] FileResourceType resourceType = FileResourceType.Auto)
    {
        var uploaderUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await uploadService.UploadFileAsync(
            file,
            folder,
            uploaderUserId,
            conversationId,
            resourceType);

        return HandleResult(result);
    }
}