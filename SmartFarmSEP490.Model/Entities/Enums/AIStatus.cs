namespace SmartFarmSEP490.Model.Enums;

/// <summary>
/// Trạng thái xử lý AI trên PlantImage (denormalize) hoặc AIAnalysis (audit).
/// Worker dùng để filter / update batch.
/// </summary>
public enum AIStatus
{
    Pending = 1,      // vừa upload, chưa gọi AI
    Processing = 2,   // Worker đang gọi AI
    Completed = 3,    // AI trả kết quả (cả khi gate fail / no detection)
    Failed = 4        // lỗi mạng, 4xx, 5xx không retry được
}
