using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Dashboard;

public interface IComparisonService
{
    // T26: Cultivation Method Comparison
    Task<CultivationComparisonDto?> GetComparisonAsync(Guid experimentId);
    Task<List<CultivationComparisonDto>> GetAllComparisonsAsync(Guid? farmId = null);

    /// <summary>
    /// So sánh chỉ số tăng trưởng trung bình giữa 2 nhóm theo từng giai đoạn.
    /// </summary>
    /// <param name="experimentId">ID của thực nghiệm.</param>
    /// <param name="groupAId">ID nhóm A (đối chứng).</param>
    /// <param name="groupBId">ID nhóm B (đối chứng).</param>
    /// <param name="metricName">Tên chỉ số cần so sánh (ví dụ: "Chiều cao", "Số lá"). Nếu null/rỗng sẽ trả về tất cả các chỉ số.</param>
    Task<GroupGrowthComparisonDto?> GetGroupGrowthComparisonAsync(
        Guid experimentId,
        Guid groupAId,
        Guid groupBId,
        string? metricName = null);
}
