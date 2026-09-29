namespace SmartFarmSEP490.Service.Interfaces.Experiments;

/// <summary>
/// Service tập trung xử lý logic "hoàn thành" một experiment:
///   - Auto-complete khi EndDate đã qua (gọi từ BackgroundService).
///   - Manual complete từ UpdateStatusAsync (status = Completed).
/// Cả hai đường đều release bed assignments để bed rảnh cho experiment mới.
/// </summary>
public interface IExperimentCompletionService
{
    /// <summary>
    /// Sweep tất cả experiment có EndDate &lt; today, đang Active hoặc Paused.
    /// Với mỗi experiment: chuyển sang Completed + release beds.
    /// Idempotent — gọi nhiều lần vẫn an toàn.
    /// Trả về số experiment đã xử lý thành công.
    /// </summary>
    Task<int> SweepExpiredAsync(CancellationToken ct = default);

    /// <summary>
    /// Hoàn tất một experiment cụ thể: chuyển Completed (nếu chưa) + release beds.
    /// Được gọi từ UpdateStatusAsync khi researcher đổi status sang Completed.
    /// </summary>
    Task CompleteAsync(Guid experimentId, CancellationToken ct = default);
}
