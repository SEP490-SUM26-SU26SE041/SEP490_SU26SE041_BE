namespace SmartFarmSEP490.Service.Interfaces.Tasks;

/// <summary>
/// Service quản lý việc tự động đánh dấu task quá hạn (DueDate &lt; now) sang status = Overdue.
/// Được gọi 2 nơi:
///   1. Lazy — ở đầu các TaskService.Get*() để user thấy status đúng ngay khi GET.
///   2. Background — OverdueTaskSweepBackgroundService chạy mỗi ngày 00:00 ICT.
/// Cả 2 đều idempotent (WHERE Status IN (Pending, InProgress)), an toàn khi chạy đồng thời.
/// </summary>
public interface IOverdueTaskService
{
    /// <summary>Chạy sweep 1 lần. Trả về số task được chuyển sang Overdue.</summary>
    Task<int> SweepAsync(CancellationToken ct = default);
}