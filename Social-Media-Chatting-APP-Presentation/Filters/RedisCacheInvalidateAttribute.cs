using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Social_Media_Chatting_APP_Presentation.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RedisCacheInvalidateAttribute(
    string prefix,
    string? targetUserRouteParam = null)   // name of the route param holding the target userId
    : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Execute the action first
        var executed = await next();

        // Only invalidate if action succeeded (2xx)
        if (executed.Result is ObjectResult { StatusCode: >= 400 } or BadRequestResult)
            return;

        var redis = context.HttpContext.RequestServices
            .GetRequiredService<IConnectionMultiplexer>();

        var db     = redis.GetDatabase();
        var server = redis.GetServer(redis.GetEndPoints().First());

        // Always invalidate current user's cache
        var currentUserId = context.HttpContext.User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        foreach (var key in server.Keys(pattern: $"{prefix}:{currentUserId}:*"))
            await db.KeyDeleteAsync(key);

        // Also invalidate target user's cache if route param provided
        if (targetUserRouteParam is not null &&
            context.ActionArguments.TryGetValue(targetUserRouteParam, out var targetId))
        {
            foreach (var key in server.Keys(pattern: $"{prefix}:{targetId}:*"))
                await db.KeyDeleteAsync(key);
        }
    }
}