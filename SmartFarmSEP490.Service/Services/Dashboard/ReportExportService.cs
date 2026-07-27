using System.Text;
using System.Text.Json;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Service.Interfaces.Dashboard;

namespace SmartFarmSEP490.Service.Services.Dashboard;

public class ReportExportService : IReportExportService
{
    private readonly SmartFarmDbContext _context;
    private readonly IExperimentRepository _experimentRepository;
    private readonly IExperimentReportRepository _reportRepository;

    public ReportExportService(
        SmartFarmDbContext context,
        IExperimentRepository experimentRepository,
        IExperimentReportRepository reportRepository)
    {
        _context = context;
        _experimentRepository = experimentRepository;
        _reportRepository = reportRepository;
    }

    public async Task<ExportReportResultDto> GenerateReportAsync(ExportReportRequestDto request, Guid userId)
    {
        var experiment = await _experimentRepository.GetByIdAsync(request.ExperimentId);
        if (experiment == null)
        {
            return new ExportReportResultDto
            {
                Status = "Failed",
                GeneratedAt = DateTime.UtcNow
            };
        }

        var resultData = BuildResultData(request, experiment);
        var summary = BuildSummary(request, experiment);

        var report = new ExperimentReport
        {
            Id = Guid.NewGuid(),
            ExperimentId = request.ExperimentId,
            CreatedBy = userId,
            ReportType = request.ReportType,
            Title = $"{request.ReportType} Report - {experiment.ExperimentCode}",
            Summary = summary,
            ResultData = JsonSerializer.Serialize(resultData),
            ExportFormat = request.ExportFormat,
            CreatedAt = DateTime.UtcNow
        };

        await _reportRepository.AddAsync(report);

        var fileUrl = GenerateFileUrl(report, request.ExportFormat);

        return new ExportReportResultDto
        {
            ReportId = report.Id,
            Title = report.Title,
            ExportFormat = request.ExportFormat,
            FileUrl = fileUrl,
            GeneratedAt = report.CreatedAt,
            FileSizeBytes = EstimateFileSize(resultData, request.ExportFormat),
            Status = "Success"
        };
    }

    public async Task<List<ExperimentReportDto>> GetExperimentReportsAsync(Guid experimentId)
    {
        var reports = await _reportRepository.GetByExperimentAsync(experimentId);
        return reports.Select(r => new ExperimentReportDto
        {
            Id = r.Id,
            ExperimentId = r.ExperimentId,
            ExperimentCode = _context.Experiments.FirstOrDefault(e => e.Id == r.ExperimentId)?.ExperimentCode ?? "",
            Title = r.Title,
            ReportType = r.ReportType,
            Summary = r.Summary,
            ResultData = r.ResultData,
            ExportFormat = r.ExportFormat,
            FileUrl = r.FileUrl,
            CreatedBy = r.CreatedBy,
            CreatedByName = r.CreatedByNavigation?.FullName,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<ExperimentReportDto?> GetReportByIdAsync(Guid reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null) return null;

        return new ExperimentReportDto
        {
            Id = report.Id,
            ExperimentId = report.ExperimentId,
            ExperimentCode = _context.Experiments.FirstOrDefault(e => e.Id == report.ExperimentId)?.ExperimentCode ?? "",
            Title = report.Title,
            ReportType = report.ReportType,
            Summary = report.Summary,
            ResultData = report.ResultData,
            ExportFormat = report.ExportFormat,
            FileUrl = report.FileUrl,
            CreatedBy = report.CreatedBy,
            CreatedByName = report.CreatedByNavigation?.FullName,
            CreatedAt = report.CreatedAt
        };
    }

    public async Task<bool> DeleteReportAsync(Guid reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null) return false;

        await _reportRepository.DeleteAsync(reportId);
        return true;
    }

    private ReportResultData BuildResultData(ExportReportRequestDto request, Experiment experiment)
    {
        var resultData = new ReportResultData
        {
            ExperimentInfo = new ExperimentInfoData
            {
                Id = experiment.Id,
                ExperimentCode = experiment.ExperimentCode,
                Title = experiment.Title,
                Objective = experiment.Objective,
                Hypothesis = experiment.Hypothesis,
                Status = experiment.Status.ToString(),
                StartDate = experiment.StartDate,
                EndDate = experiment.EndDate,
                CreatedAt = experiment.CreatedAt
            }
        };

        if (request.IncludeGroups)
        {
            var groups = _context.ExperimentGroups.Where(g => g.ExperimentId == experiment.Id).ToList();
            resultData.Groups = groups.Select(g => new GroupSummaryData
            {
                Id = g.Id,
                GroupName = g.GroupName,
                GroupType = g.GroupType.ToString(),
                TreatmentDescription = g.TreatmentDescription,
                BatchCount = _context.Batches.Count(b => b.GroupId == g.Id)
            }).ToList();
        }

        if (request.IncludeMeasurements)
        {
            var measurements = _context.MeasurementRecords
                .Where(m => m.ExperimentId == experiment.Id)
                .OrderByDescending(m => m.MeasuredAt)
                .Take(1000)
                .ToList();

            var metricGroups = measurements
                .Where(m => m.MeasurementDefinition != null)
                .GroupBy(m => m.MeasurementDefinition!.MetricName)
                .Select(g =>
                {
                    var values = g.Where(m => m.Value.HasValue).Select(m => m.Value!.Value).ToList();
                    return new MetricSummaryItem
                    {
                        MetricName = g.Key,
                        RecordCount = g.Count(),
                        AverageValue = values.Any() ? Math.Round(values.Average(), 4) : 0,
                        MinValue = values.Any() ? values.Min() : 0,
                        MaxValue = values.Any() ? values.Max() : 0
                    };
                }).ToList();

            resultData.MeasurementSummary = new MeasurementSummaryData
            {
                TotalRecords = measurements.Count,
                Metrics = metricGroups
            };
        }

        if (request.IncludeTasks)
        {
            var tasks = _context.Tasks
                .Where(t => t.ExperimentId == experiment.Id)
                .ToList();

            resultData.TaskSummary = new TaskSummaryData
            {
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == Model.Enums.TaskStatus.Completed),
                PendingTasks = tasks.Count(t => t.Status == Model.Enums.TaskStatus.Pending),
                InProgressTasks = tasks.Count(t => t.Status == Model.Enums.TaskStatus.InProgress),
                OverdueTasks = tasks.Count(t => t.Status == Model.Enums.TaskStatus.Overdue),
                ByTaskType = tasks
                    .GroupBy(t => t.Type)
                    .Select(g => new TaskTypeSummary
                    {
                        TaskType = g.Key.ToString(),
                        Total = g.Count(),
                        Completed = g.Count(t => t.Status == Model.Enums.TaskStatus.Completed)
                    }).ToList()
            };
        }

        if (request.IncludeStatistics)
        {
            var batches = _context.Batches.Where(b => b.ExperimentId == experiment.Id).ToList();
            var batchIds = batches.Select(b => b.Id).ToList();
            var allMeasurements = _context.MeasurementRecords
                .Where(m => batchIds.Contains(m.BatchId))
                .ToList();

            var values = allMeasurements.Where(m => m.Value.HasValue).Select(m => m.Value!.Value).ToList();
            if (values.Count > 0)
            {
                resultData.StatisticalSummary = new StatisticalData
                {
                    TotalSamples = values.Count,
                    Mean = Math.Round(values.Average(), 4),
                    StandardDeviation = CalculateStdDev(values),
                    Min = values.Min(),
                    Max = values.Max(),
                    Range = values.Max() - values.Min()
                };
            }
        }

        return resultData;
    }

    private string BuildSummary(ExportReportRequestDto request, Experiment experiment)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Report Type: {request.ReportType}");
        sb.AppendLine($"Experiment: {experiment.ExperimentCode} - {experiment.Title}");
        sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        var stages = _context.ExperimentStages.Where(s => s.ExperimentId == experiment.Id).ToList();
        sb.AppendLine($"Total Stages: {stages.Count}");
        sb.AppendLine($"Experiment Status: {experiment.Status}");

        if (request.IncludeGroups)
        {
            var groupCount = _context.ExperimentGroups.Count(g => g.ExperimentId == experiment.Id);
            sb.AppendLine($"Total Groups: {groupCount}");
        }

        if (request.IncludeMeasurements)
        {
            var measurementCount = _context.MeasurementRecords.Count(m => m.ExperimentId == experiment.Id);
            sb.AppendLine($"Total Measurements: {measurementCount}");
        }

        if (request.IncludeTasks)
        {
            var taskCount = _context.Tasks.Count(t => t.ExperimentId == experiment.Id);
            var completedCount = _context.Tasks.Count(t => t.ExperimentId == experiment.Id && t.Status == Model.Enums.TaskStatus.Completed);
            sb.AppendLine($"Total Tasks: {taskCount} (Completed: {completedCount})");
        }

        return sb.ToString();
    }

    private string GenerateFileUrl(ExperimentReport report, string format)
    {
        var baseUrl = "/api/reports/download";
        return $"{baseUrl}/{report.Id}?format={format.ToLower()}";
    }

    private long EstimateFileSize(ReportResultData data, string format)
    {
        var json = JsonSerializer.Serialize(data);
        var baseSize = Encoding.UTF8.GetByteCount(json);

        return format.ToUpper() switch
        {
            "PDF" => (long)(baseSize * 1.5),
            "EXCEL" => (long)(baseSize * 1.2),
            "CSV" => (long)(baseSize * 0.8),
            _ => baseSize
        };
    }

    private decimal CalculateStdDev(List<decimal> values)
    {
        if (values.Count == 0) return 0;
        var avg = values.Average();
        var sumOfSquares = values.Sum(v => (v - avg) * (v - avg));
        return (decimal)Math.Sqrt((double)(sumOfSquares / values.Count));
    }
}

public class ReportResultData
{
    public ExperimentInfoData? ExperimentInfo { get; set; }
    public List<GroupSummaryData> Groups { get; set; } = new();
    public MeasurementSummaryData? MeasurementSummary { get; set; }
    public TaskSummaryData? TaskSummary { get; set; }
    public StatisticalData? StatisticalSummary { get; set; }
}

public class ExperimentInfoData
{
    public Guid Id { get; set; }
    public string ExperimentCode { get; set; } = "";
    public string Title { get; set; } = "";
    public string Objective { get; set; } = "";
    public string? Hypothesis { get; set; }
    public string Status { get; set; } = "";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GroupSummaryData
{
    public Guid Id { get; set; }
    public string GroupName { get; set; } = "";
    public string GroupType { get; set; } = "";
    public string? TreatmentDescription { get; set; }
    public int BatchCount { get; set; }
}

public class MeasurementSummaryData
{
    public int TotalRecords { get; set; }
    public List<MetricSummaryItem> Metrics { get; set; } = new();
}

public class MetricSummaryItem
{
    public string MetricName { get; set; } = "";
    public int RecordCount { get; set; }
    public decimal AverageValue { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
}

public class TaskSummaryData
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public List<TaskTypeSummary> ByTaskType { get; set; } = new();
}

public class TaskTypeSummary
{
    public string TaskType { get; set; } = "";
    public int Total { get; set; }
    public int Completed { get; set; }
}

public class StatisticalData
{
    public int TotalSamples { get; set; }
    public decimal Mean { get; set; }
    public decimal StandardDeviation { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Range { get; set; }
}
