using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Dashboard;

public interface IDashboardService
{
    // T24: Real-time Monitoring
    Task<DashboardOverviewDto> GetDashboardOverviewAsync(Guid? farmId = null);
    Task<List<FarmHealthDto>> GetFarmHealthListAsync();
    Task<FarmHealthDto?> GetFarmHealthAsync(Guid farmId);
    Task<List<LatestSensorReadingDto>> GetLatestSensorReadingsAsync(Guid? farmId = null, Guid? experimentId = null);
    Task<List<SensorReadingDto>> GetSensorHistoryAsync(Guid sensorId, DateTime? fromDate = null, DateTime? toDate = null, int limit = 100);
    Task<List<AlertSummaryDto>> GetActiveAlertsAsync(Guid? experimentId = null);
    Task<List<ExperimentStatusDto>> GetExperimentStatusesAsync(Guid? farmId = null);

    // T25: KPIs and Personnel Performance
    Task<DashboardKpiDto> GetKpisAsync(Guid? farmId = null, Guid? experimentId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<PersonnelPerformanceDto>> GetPersonnelPerformanceAsync(Guid? farmId = null, Guid? experimentId = null);
    Task<PersonnelPerformanceDto?> GetPersonnelPerformanceByIdAsync(Guid userId);
    Task<List<ExperimentProgressDto>> GetExperimentProgressAsync(Guid? farmId = null);
    Task<ExperimentProgressDto?> GetExperimentProgressByIdAsync(Guid experimentId);
}
