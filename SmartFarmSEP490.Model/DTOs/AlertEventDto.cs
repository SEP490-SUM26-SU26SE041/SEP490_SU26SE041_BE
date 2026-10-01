using System;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model.DTOs;

/// <summary>
/// DTO gửi qua WebSocket khi có Alert sensor mới (event "ReceiveAlert").
/// Dùng để Dashboard/UI cập nhật alert realtime (badge, bell, toast).
/// </summary>
/// <remarks>
/// <para>Cấu trúc envelope chung (do <c>WebSocketConnectionManager</c> đóng gói):</para>
/// <code>
/// {
///   "event": "ReceiveAlert",
///   "data": { ...AlertEventDto },
///   "ts": "2026-09-30T05:00:00Z",
///   "tsVietnam": "2026-09-30T12:00:00+07:00"
/// }
/// </code>
/// </remarks>
public class AlertEventDto
{
    /// <summary>ID của Alert.</summary>
    public Guid AlertId { get; set; }

    /// <summary>ID của Experiment liên quan.</summary>
    public Guid? ExperimentId { get; set; }

    /// <summary>ID của Batch (nếu có).</summary>
    public Guid? BatchId { get; set; }

    /// <summary>ID của Sensor.</summary>
    public Guid? SensorId { get; set; }

    /// <summary>Mã sensor (ví dụ "DHT-TEMP-001").</summary>
    public string? SensorCode { get; set; }

    /// <summary>Loại cảm biến (Temperature, Humidity...).</summary>
    public SensorType? SensorType { get; set; }

    /// <summary>Tiêu đề alert (đã format sẵn).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Nội dung chi tiết.</summary>
    public string? Message { get; set; }

    /// <summary>Mức độ nghiêm trọng (Low/Medium/High/Critical).</summary>
    public AlertSeverity Severity { get; set; }

    /// <summary>Thời điểm tạo alert (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Thời điểm tạo alert (UTC+7) — cho FE hiển thị.</summary>
    public DateTimeOffset CreatedAtVietnam { get; set; }
}