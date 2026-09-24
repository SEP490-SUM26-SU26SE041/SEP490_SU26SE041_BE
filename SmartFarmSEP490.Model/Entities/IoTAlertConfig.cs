using System;

namespace SmartFarmSEP490.Model;

/// <summary>
/// Cấu hình cảnh báo khi thiết bị IoT offline.
/// Singleton: thường chỉ có 1 row duy nhất.
/// </summary>
public partial class IoTAlertConfig
{
    public Guid Id { get; set; }

    /// <summary>Sau bao nhiêu phút không có data → tính là offline</summary>
    public int OfflineThresholdMinutes { get; set; } = 5;

    /// <summary>Mỗi bao nhiêu giây kiểm tra 1 lần</summary>
    public int CheckIntervalSeconds { get; set; } = 60;

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
