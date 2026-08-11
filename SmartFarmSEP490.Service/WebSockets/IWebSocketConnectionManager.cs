using System.Net.WebSockets;

namespace SmartFarmSEP490.Service.WebSockets;

/// <summary>
/// Quản lý tập các WebSocket connection đang mở theo UserId.
/// Singleton lifetime.
/// </summary>
public interface IWebSocketConnectionManager
{
    void Add(Guid userId, WebSocket socket);
    void Remove(Guid userId, WebSocket socket);

    /// <summary>Số connection hiện tại của user.</summary>
    int CountForUser(Guid userId);

    /// <summary>Gửi JSON payload tới tất cả connection của user. Trả về số message đã gửi thành công.</summary>
    Task<int> SendToUserAsync(Guid userId, string eventName, object payload, CancellationToken ct = default);
}
