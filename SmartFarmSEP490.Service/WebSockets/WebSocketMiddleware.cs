using System.Net.WebSockets;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmartFarmSEP490.Service.WebSockets;

/// <summary>
/// Middleware chấp nhận WebSocket upgrade tại <c>/ws</c>.
/// Authenticate bằng JWT qua scheme Bearer (dùng lại config AddJwtBearer trong Program.cs).
/// Token được truyền qua query <c>?token=&lt;jwt&gt;</c> (giống cách SignalR hub trước đó dùng <c>access_token</c>).
/// </summary>
public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<WebSocketMiddleware> _logger;

    public WebSocketMiddleware(RequestDelegate next, ILogger<WebSocketMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.Equals("/ws", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("WebSocket requests only.");
            return;
        }

        // Authenticate bằng JWT từ query "?token="
        var token = context.Request.Query["token"].ToString();
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing ?token=<jwt>.");
            return;
        }

        AuthenticateResult authResult;
        try
        {
            authResult = await context.AuthenticateAsync("Bearer");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[WS] Authenticate threw");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid token.");
            return;
        }

        if (!authResult.Succeeded || authResult.Principal == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid token.");
            return;
        }

        // Gán Principal để code sau dùng
        context.User = authResult.Principal;
        var userIdClaim = authResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? authResult.Principal.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token missing user id.");
            return;
        }

        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var manager = context.RequestServices.GetRequiredService<IWebSocketConnectionManager>();

        manager.Add(userId, socket);

        // Gửi hello message
        await manager.SendToUserAsync(userId, "Connected", new { UserId = userId }, default);

        try
        {
            await ReceiveLoopAsync(context, socket, userId);
        }
        finally
        {
            manager.Remove(userId, socket);
            try
            {
                if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closing", CancellationToken.None);
                }
            }
            catch { /* ignore */ }
        }
    }

    /// <summary>
    /// Vòng lặp nhận message từ client. Mục đích chính là giữ connection alive và detect disconnect.
    /// Client có thể gửi các message bất kỳ (ví dụ "ping"); server không broadcast các message này.
    /// </summary>
    private async Task ReceiveLoopAsync(HttpContext context, WebSocket socket, Guid userId)
    {
        var buffer = new byte[4 * 1024];
        while (socket.State == WebSocketState.Open)
        {
            var ct = context.RequestAborted;
            WebSocketReceiveResult result;
            try
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
            }
            catch (OperationCanceledException) { break; }
            catch (WebSocketException) { break; }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }
            // Có thể xử lý ping/pong ở đây nếu sau này cần
        }
    }
}
