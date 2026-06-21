using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using StackExchange.Redis;

namespace Social_Media_Chatting_APP_Presentation.Filters;

/// <summary>
/// Invalidates Redis cache for BOTH the current user and the requester
/// by reading RequesterId from the FriendshipActionResultDto response.
/// Use this on endpoints where the target user ID is not in the route (e.g. respond).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RedisCacheInvalidateWithResponseAttribute(string prefix)
    : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Run the action first
        var executed = await next();

        // Only invalidate on success (2xx)
        if (executed.Result is not ObjectResult { StatusCode: 200 or null } objectResult)
            return;

        var redis  = context.HttpContext.RequestServices
            .GetRequiredService<IConnectionMultiplexer>();
        var db     = redis.GetDatabase();
        var server = redis.GetServer(redis.GetEndPoints().First());

        // Invalidate current user's cache
        var currentUserId = context.HttpContext.User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        foreach (var key in server.Keys(pattern: $"{prefix}:{currentUserId}:*"))
            await db.KeyDeleteAsync(key);

        // Invalidate requester's cache by reading RequesterId from the response DTO
        if (objectResult.Value is FriendshipActionResultDto dto)
        {
            var requesterId = dto.RequesterId.ToString();
            foreach (var key in server.Keys(pattern: $"{prefix}:{requesterId}:*"))
                await db.KeyDeleteAsync(key);
        }
    }
}
