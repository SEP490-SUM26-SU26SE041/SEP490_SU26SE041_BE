using System.Collections.Concurrent;

namespace SmartFarmSEP490.API.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, IdempotencyEntry> _cache = new();

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != HttpMethods.Post)
        {
            await _next(context);
            return;
        }

        var idempotencyKey = context.Request.Headers["Idempotency-Key"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _next(context);
            return;
        }

        if (_cache.TryGetValue(idempotencyKey, out var existingEntry))
        {
            if (existingEntry.Response != null)
            {
                context.Response.StatusCode = existingEntry.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(existingEntry.Response);
                return;
            }

            context.Response.StatusCode = 409;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"type\":\"https://tools.ietf.org/html/rfc9110#section-8.3\",\"title\":\"Conflict\",\"detail\":\"A request with this Idempotency-Key is currently being processed.\"}");
            return;
        }

        var entry = new IdempotencyEntry();
        if (!_cache.TryAdd(idempotencyKey, entry))
        {
            await _next(context);
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();

            entry.Response = responseText;
            entry.StatusCode = context.Response.StatusCode;

            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch
        {
            _cache.TryRemove(idempotencyKey, out _);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private class IdempotencyEntry
    {
        public string? Response { get; set; }
        public int StatusCode { get; set; }
    }
}

public static class IdempotencyMiddlewareExtensions
{
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IdempotencyMiddleware>();
    }
}
