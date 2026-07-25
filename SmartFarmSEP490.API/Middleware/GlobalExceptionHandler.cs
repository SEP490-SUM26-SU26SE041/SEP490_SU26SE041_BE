using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace SmartFarmSEP490.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, errorCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "FORBIDDEN", "Access denied."),
            ArgumentException argEx => (HttpStatusCode.BadRequest, "BAD_REQUEST", argEx.Message),
            KeyNotFoundException notFoundEx => (HttpStatusCode.NotFound, "NOT_FOUND", notFoundEx.Message),
            InvalidOperationException invEx => (HttpStatusCode.BadRequest, "INVALID_OPERATION", invEx.Message),
            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "CONCURRENCY_CONFLICT", "The resource has been modified by another user. Please refresh and try again."),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", _env.IsDevelopment() ? exception.Message : "An unexpected error occurred.")
        };

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = (int)statusCode,
            Title = errorCode,
            Detail = message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        if (_env.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            }),
            cancellationToken);

        return true;
    }
}
