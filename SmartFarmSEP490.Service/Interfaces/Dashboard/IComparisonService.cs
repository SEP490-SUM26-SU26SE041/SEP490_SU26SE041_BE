using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Dashboard;

public interface IComparisonService
{
    // T26: Cultivation Method Comparison
    Task<CultivationComparisonDto?> GetComparisonAsync(Guid experimentId);
    Task<List<CultivationComparisonDto>> GetAllComparisonsAsync(Guid? farmId = null);
}
