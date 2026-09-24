namespace SmartFarmSEP490.Model.Enums;

/// <summary>
/// Trạng thái hoạt động của thiết bị IoT.
/// - Inactive: Chưa gán vào Batch hoặc đã tắt thủ công
/// - Active:   Đang hoạt động (đã gán vào Batch có IsIoTEnabled=true)
/// </summary>
public enum IoTDeviceStatus
{
    /// <summary>Chưa kích hoạt / Chưa gán vào Batch</summary>
    Inactive = 0,

    /// <summary>Đang hoạt động (đã gán vào Batch và nhận data)</summary>
    Active = 1
}
