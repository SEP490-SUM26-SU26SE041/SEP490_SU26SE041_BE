namespace SmartFarmSEP490.Model.Enums;

public enum ExperimentStatus
{
    Active = 1,
    Completed = 2,
    Paused = 3,
    Cancelled = 4
}
// 4 trạng thái: Active, Completed, Paused, Cancelled
// - Active:     experiment đang chạy (đã được duyệt request -> tự động chuyển sang Active)
// - Completed:  experiment đã hoàn thành -> tự động chuyển sang Completed
// - Paused:     experiment đang tạm dừng (có thể resume lại về Active)
// - Cancelled:  experiment đã bị hủy (trạng thái kết thúc, không thể resume)
