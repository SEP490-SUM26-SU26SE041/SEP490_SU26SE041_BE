using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SmartFarmSEP490.Service.WebSockets;

public class WebSocketConnectionManager : IWebSocketConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<WebSocket, byte>> _connections = new();
    private readonly ILogger<WebSocketConnectionManager> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
    {
        _logger = logger;
    }

    public void Add(Guid userId, WebSocket socket)
    {
        var bag = _connections.GetOrAdd(userId, _ => new ConcurrentDictionary<WebSocket, byte>());
        bag.TryAdd(socket, 0);
        _logger.LogInformation("[WS] Connected userId={UserId}, total for user={Count}", userId, bag.Count);
    }

    public void Remove(Guid userId, WebSocket socket)
    {
        if (_connections.TryGetValue(userId, out var bag))
        {
            bag.TryRemove(socket, out _);
            if (bag.IsEmpty)
            {
                _connections.TryRemove(userId, out _);
            }
        }
        _logger.LogInformation("[WS] Disconnected userId={UserId}", userId);
    }

    public int CountForUser(Guid userId)
        => _connections.TryGetValue(userId, out var bag) ? bag.Count : 0;

    public async Task<int> SendToUserAsync(Guid userId, string eventName, object payload, CancellationToken ct = default)
    {
        if (!_connections.TryGetValue(userId, out var bag) || bag.IsEmpty)
            return 0;

        var envelope = new
        {
            @event = eventName,
            data = payload,
            ts = DateTime.UtcNow
        };
        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        var sent = 0;
        foreach (var ws in bag.Keys)
        {
            if (ws.State != WebSocketState.Open) continue;
            try
            {
                await ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
                sent++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[WS] Failed to send to userId={UserId}", userId);
            }
        }
        return sent;
    }
}
