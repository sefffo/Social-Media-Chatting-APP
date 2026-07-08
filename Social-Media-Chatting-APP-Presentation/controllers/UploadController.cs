using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_Service.Common.Upload;

namespace Social_Media_Chatting_APP_Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController(
    UploadService uploadService
) : ApiBaseController
{
    [Authorize]
    public async Task UploadFile([FromBody] IFormFile file)
    {
        
        //first check the user uploading that 

        var userId = await Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


    }
}