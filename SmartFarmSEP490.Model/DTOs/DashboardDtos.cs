namespace SmartFarmSEP490.Model.DTOs;

// ============ T24: Real-time Dashboard Monitoring DTOs ============

public class DashboardOverviewDto
{
    public int TotalExperiments { get; set; }
    public int ActiveExperiments { get; set; }
    public int TotalBatches { get; set; }
    public int ActiveBatches { get; set; }
    public int TotalAreas { get; set; }
    public int ActiveAreas { get; set; }
    public int TotalBeds { get; set; }
    public int ActiveBeds { get; set; }
    public int TotalSensors { get; set; }
    public int ActiveSensors { get; set; }
    public int ActiveAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class FarmHealthDto
{
    public Guid FarmId { get; set; }
    public string FarmCode { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int TotalAreas { get; set; }
    public int TotalExperiments { get; set; }
    public int ActiveExperiments { get; set; }
    public int TotalBatches { get; set; }
    public int ActiveAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public double HealthScore { get; set; }
    public string Status { get; set; } = "Healthy";
}

public class SensorReadingDto
{
    public Guid Id { get; set; }
    public Guid SensorId { get; set; }
    public string SensorCode { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; }
    public Guid? ExperimentId { get; set; }
    public Guid? BatchId { get; set; }
}

public class LatestSensorReadingDto
{
    public Guid SensorId { get; set; }
    public string SensorCode { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public decimal LatestValue { get; set; }
    public string? Unit { get; set; }
    public DateTime LastRecordedAt { get; set; }
    public decimal? MinThreshold { get; set; }
    public decimal? MaxThreshold { get; set; }
    public string Status { get; set; } = "Normal";
}

public class ExperimentStatusDto
{
    public Guid ExperimentId { get; set; }
    public string ExperimentCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid FarmId { get; set; }
    public string? FarmName { get; set; }
    public Guid ResearcherId { get; set; }
    public string? ResearcherName { get; set; }
    public int TotalStages { get; set; }
    public int CompletedStages { get; set; }
    public int TotalBatches { get; set; }
    public int ActiveBatches { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int ActiveAlerts { get; set; }
    public double ProgressPercentage { get; set; }
}

public class AlertSummaryDto
{
    public Guid AlertId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Severity { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ExperimentId { get; set; }
    public string? ExperimentCode { get; set; }
    public Guid? SensorId { get; set; }
    public string? SensorCode { get; set; }
    public Guid? BatchId { get; set; }
    public string? BatchCode { get; set; }
}

// ============ T25: KPIs and Personnel Performance DTOs ============

public class DashboardKpiDto
{
    public Guid? FarmId { get; set; }
    public Guid? ExperimentId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int InProgressTasks { get; set; }
    public double TaskCompletionRate { get; set; }
    public double OnTimeCompletionRate { get; set; }

    public int TotalExperiments { get; set; }
    public int ActiveExperiments { get; set; }
    public int CompletedExperiments { get; set; }

    public int TotalBatches { get; set; }
    public int ActiveBatches { get; set; }
    public int HarvestedBatches { get; set; }

    public int TotalMeasurementRecords { get; set; }
    public List<DailyMetricDto> DailyCompletions { get; set; } = new();
}

public class DailyMetricDto
{
    public DateOnly Date { get; set; }
    public int TasksCompleted { get; set; }
    public int TasksCreated { get; set; }
    public int MeasurementsRecorded { get; set; }
}

public class PersonnelPerformanceDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int TotalTasksAssigned { get; set; }
    public int TasksCompleted { get; set; }
    public int TasksInProgress { get; set; }
    public int TasksOverdue { get; set; }
    public int TasksPending { get; set; }
    public double CompletionRate { get; set; }
    public double OnTimeCompletionRate { get; set; }
    public double AverageCompletionDays { get; set; }
    public DateTime LastActivityAt { get; set; }
    public List<TaskTypePerformanceDto> TaskTypeBreakdown { get; set; } = new();
}

public class TaskTypePerformanceDto
{
    public string TaskType { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int Completed { get; set; }
    public double CompletionRate { get; set; }
}

public class ExperimentProgressDto
{
    public Guid ExperimentId { get; set; }
    public string ExperimentCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int CurrentDay { get; set; }
    public int TotalDays { get; set; }
    public double OverallProgress { get; set; }
    public List<StageProgressDto> StageProgress { get; set; } = new();
    public List<GroupProgressDto> GroupProgress { get; set; } = new();
}

public class StageProgressDto
{
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string StageType { get; set; } = string.Empty;
    public int StageOrder { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double ProgressPercentage { get; set; }
}

public class GroupProgressDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string GroupType { get; set; } = string.Empty;
    public string? TreatmentDescription { get; set; }
    public int TotalBatches { get; set; }
    public int TotalMeasurementRecords { get; set; }
    public decimal? AverageMetricValue { get; set; }
    public string? MetricName { get; set; }
}

// ============ T26: Cultivation Method Comparison DTOs ============

public class CultivationComparisonDto
{
    public Guid ExperimentId { get; set; }
    public string ExperimentCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Hypothesis { get; set; }
    public string DesignType { get; set; } = string.Empty;
    public int ReplicationCount { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<GroupComparisonDto> GroupComparisons { get; set; } = new();
    public StatisticalSummaryDto StatisticalSummary { get; set; } = new();
}

public class GroupComparisonDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string GroupType { get; set; } = string.Empty;
    public string? TreatmentDescription { get; set; }
    public int TotalBatches { get; set; }
    public int TotalMeasurements { get; set; }
    public List<MetricComparisonDto> MetricComparisons { get; set; } = new();
    public List<BatchMetricDto> BatchMetrics { get; set; } = new();
    public MetricStatisticsDto Statistics { get; set; } = new();
}

public class MetricComparisonDto
{
    public string MetricName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? AverageValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? StandardDeviation { get; set; }
    public decimal? Variance { get; set; }
    public int SampleSize { get; set; }
    public int MeasurementsWithinTarget { get; set; }
    public double TargetAchievementRate { get; set; }
}

public class BatchMetricDto
{
    public Guid BatchId { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly? PlantingDate { get; set; }
    public DateOnly? ExpectedHarvestDate { get; set; }
    public int PlantCount { get; set; }
    public decimal? AverageMetricValue { get; set; }
    public int MeasurementCount { get; set; }
    public List<MetricTimeSeriesDto> MetricTimeSeries { get; set; } = new();
}

public class MetricTimeSeriesDto
{
    public DateTime RecordedAt { get; set; }
    public decimal Value { get; set; }
}

public class MetricStatisticsDto
{
    public decimal? Mean { get; set; }
    public decimal? Median { get; set; }
    public decimal? Mode { get; set; }
    public decimal? StandardDeviation { get; set; }
    public decimal? Variance { get; set; }
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public decimal? Range { get; set; }
    public int Count { get; set; }
    public double? CoefficientOfVariation { get; set; }
}

public class StatisticalSummaryDto
{
    public bool IsSignificant { get; set; }
    public double? PValue { get; set; }
    public string? StatisticalTest { get; set; }
    public string? Conclusion { get; set; }
    public string? Recommendation { get; set; }
    public List<string> KeyFindings { get; set; } = new();
}

// ============ T27: Report Export DTOs ============

public class ExperimentReportDto
{
    public Guid Id { get; set; }
    public Guid ExperimentId { get; set; }
    public string ExperimentCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ResultData { get; set; }
    public string? ExportFormat { get; set; }
    public string? FileUrl { get; set; }
    public Guid? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExperimentReportDto
{
    public Guid ExperimentId { get; set; }
    public string ReportType { get; set; } = "Summary";
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ResultData { get; set; }
    public string? ExportFormat { get; set; }
}

public class ExportReportRequestDto
{
    public Guid ExperimentId { get; set; }
    public string ReportType { get; set; } = "Summary";
    public string ExportFormat { get; set; } = "PDF";
    public bool IncludeMeasurements { get; set; } = true;
    public bool IncludeTasks { get; set; } = true;
    public bool IncludeGroups { get; set; } = true;
    public bool IncludeStatistics { get; set; } = true;
    public bool IncludeCharts { get; set; } = false;
}

public class ExportReportResultDto
{
    public Guid ReportId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ExportFormat { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public long FileSizeBytes { get; set; }
    public string Status { get; set; } = "Success";
}

// ============ Group-vs-Group Growth Comparison (per stage) DTOs ============

/// <summary>
/// So sánh chỉ số tăng trưởng trung bình giữa 2 nhóm theo từng giai đoạn
/// (per stage, có thể là theo chiều cao, số lá, hoặc tất cả metric).
/// </summary>
public class GroupGrowthComparisonDto
{
    public Guid ExperimentId { get; set; }
    public string ExperimentCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public GroupComparisonSideDto GroupA { get; set; } = new();
    public GroupComparisonSideDto GroupB { get; set; } = new();

    /// <summary>
    /// Danh sách giai đoạn của thực nghiệm, mỗi giai đoạn có trung bình chỉ số tăng trưởng của 2 nhóm.
    /// </summary>
    public List<StageGrowthComparisonDto> StageComparisons { get; set; } = new();
}

/// <summary>
/// Thông tin một nhóm (phía A hoặc phía B) trong phép so sánh.
/// </summary>
public class GroupComparisonSideDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string GroupType { get; set; } = string.Empty;
    public string? TreatmentDescription { get; set; }
    public int TotalBatches { get; set; }
    public int TotalMeasurementRecords { get; set; }
}

/// <summary>
/// So sánh một giai đoạn giữa 2 nhóm (cho một metric - ví dụ chiều cao hoặc số lá).
/// </summary>
public class StageGrowthComparisonDto
{
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int StageOrder { get; set; }
    public string StageType { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Danh sách các chỉ số (metric) được so sánh trong giai đoạn này.
    /// Nếu người dùng truyền <c>metricName</c> thì danh sách này chỉ chứa 1 phần tử.
    /// </summary>
    public List<MetricStageComparisonDto> MetricComparisons { get; set; } = new();
}

/// <summary>
/// So sánh một chỉ số (height / leaf count / ...) giữa 2 nhóm trong một giai đoạn.
/// </summary>
public class MetricStageComparisonDto
{
    public Guid MeasurementDefinitionId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }

    public decimal? GroupAAverage { get; set; }
    public decimal? GroupBAverage { get; set; }
    public decimal? Difference => (GroupAAverage.HasValue && GroupBAverage.HasValue)
        ? GroupBAverage.Value - GroupAAverage.Value
        : null;
    public double? DifferencePercent => (GroupAAverage.HasValue && GroupBAverage.HasValue && GroupAAverage.Value != 0)
        ? Math.Round((double)(GroupBAverage.Value - GroupAAverage.Value) / (double)GroupAAverage.Value * 100, 2)
        : null;

    public int GroupASampleSize { get; set; }
    public int GroupBSampleSize { get; set; }
}
