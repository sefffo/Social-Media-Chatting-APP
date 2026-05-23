using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_Presentation.Filters;
using Social_Media_Chatting_APP_Service.Features.SearchUsers.Queries;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;

namespace Social_Media_Chatting_APP_Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserSearchController(ISender sender) : ApiBaseController
{
    [Authorize]
    [RedisCache("user-search", ttlSeconds: 120)]  // cache for 2 minutes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSearchResultDto>>> SearchUsers([FromQuery] string q)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new SearchUsersQuery(q, userId));
        return HandleResult(result);
    }
}