using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace SmartFarmSEP490.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                  ?? context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? context.Connection.RemoteIpAddress?.ToString()
                  ?? "anonymous";

        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "/";
        var method = context.Request.Method;

        var rateLimitKey = $"ratelimit:{userId}:{method}:{path}";

        var requestCounts = _cache.GetOrCreate(rateLimitKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return new RequestCount { Count = 0, WindowStart = DateTime.UtcNow };
        });

        if (requestCounts == null)
        {
            requestCounts = new RequestCount { Count = 0, WindowStart = DateTime.UtcNow };
        }

        var maxRequestsPerMinute = GetRateLimitForEndpoint(path);

        if (requestCounts.Count >= maxRequestsPerMinute)
        {
            var retryAfter = 60 - (int)(DateTime.UtcNow - requestCounts.WindowStart).TotalSeconds;

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.Append("Retry-After", retryAfter.ToString());
            context.Response.Headers.Append("X-RateLimit-Limit", maxRequestsPerMinute.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", "0");
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too Many Requests",
                Detail = $"Rate limit exceeded. Maximum {maxRequestsPerMinute} requests per minute allowed.",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        requestCounts.Count++;

        context.Response.Headers.Append("X-RateLimit-Limit", maxRequestsPerMinute.ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", Math.Max(0, maxRequestsPerMinute - requestCounts.Count).ToString());

        await _next(context);
    }

    private static int GetRateLimitForEndpoint(string path)
    {
        if (path.Contains("/api/tasks/generate"))
            return 10;
        if (path.Contains("/api/measurement-records"))
            return 60;
        if (path.Contains("/api/task-reports"))
            return 30;
        if (path.Contains("/api/experiments"))
            return 30;
        return 100;
    }

    private class RequestCount
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
