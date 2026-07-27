using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Alerts;
using SmartFarmSEP490.Repository.Interfaces.Batches;
using SmartFarmSEP490.Repository.Interfaces.ExperimentStages;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Repository.Interfaces.Farms;
using SmartFarmSEP490.Repository.Interfaces.Sensors;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SmartFarmSEP490.Service.Interfaces.Dashboard;

namespace SmartFarmSEP490.Service.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly SmartFarmDbContext _context;
    private readonly IExperimentRepository _experimentRepository;
    private readonly IExperimentStageRepository _stageRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskAssignmentRepository _assignmentRepository;
    private readonly ISensorRepository _sensorRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IFarmRepository _farmRepository;

    public DashboardService(
        SmartFarmDbContext context,
        IExperimentRepository experimentRepository,
        IExperimentStageRepository stageRepository,
        IBatchRepository batchRepository,
        ITaskRepository taskRepository,
        ITaskAssignmentRepository assignmentRepository,
        ISensorRepository sensorRepository,
        IAlertRepository alertRepository,
        IFarmRepository farmRepository)
    {
        _context = context;
        _experimentRepository = experimentRepository;
        _stageRepository = stageRepository;
        _batchRepository = batchRepository;
        _taskRepository = taskRepository;
        _assignmentRepository = assignmentRepository;
        _sensorRepository = sensorRepository;
        _alertRepository = alertRepository;
        _farmRepository = farmRepository;
    }

    // ========== T24: Real-time Monitoring ==========

    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(Guid? farmId = null)
    {
        var experiments = await _experimentRepository.GetAllAsync();
        var batches = _context.Batches.ToList();
        var areas = _context.Areas.ToList();
        var sensors = await _sensorRepository.GetAllAsync();
        var activeAlerts = await _alertRepository.GetActiveAlertsAsync();

        if (farmId.HasValue)
        {
            var farmExperiments = experiments.Where(e => e.FarmId == farmId.Value).ToList();
            experiments = farmExperiments;
            var farmExperimentIds = farmExperiments.Select(e => e.Id).ToList();
            batches = batches.Where(b => farmExperimentIds.Contains(b.ExperimentId)).ToList();
            areas = areas.Where(a => a.FarmId == farmId.Value).ToList();
            activeAlerts = activeAlerts.Where(a => farmExperimentIds.Contains(a.ExperimentId ?? Guid.Empty)).ToList();
        }

        return new DashboardOverviewDto
        {
            TotalExperiments = experiments.Count,
            ActiveExperiments = experiments.Count(e => e.Status == ExperimentStatus.Active),
            TotalBatches = batches.Count,
            ActiveBatches = batches.Count(b => b.Status == BatchStatus.Growing),
            TotalAreas = areas.Count,
            ActiveAreas = areas.Count(a => a.Status == LocationStatus.InUse),
            TotalBeds = areas.Sum(a => _context.Beds.Count(b => b.AreaId == a.Id)),
            ActiveBeds = batches.Count,
            TotalSensors = sensors.Count,
            ActiveSensors = sensors.Count,
            ActiveAlerts = activeAlerts.Count,
            CriticalAlerts = activeAlerts.Count(a => a.Severity == AlertSeverity.Critical),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<List<FarmHealthDto>> GetFarmHealthListAsync()
    {
        var farms = await _farmRepository.GetAllAsync();
        var result = new List<FarmHealthDto>();

        foreach (var farm in farms)
        {
            var health = await GetFarmHealthAsync(farm.Id);
            if (health != null)
                result.Add(health);
        }

        return result;
    }

    public async Task<FarmHealthDto?> GetFarmHealthAsync(Guid farmId)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId);
        if (farm == null) return null;

        var experiments = (await _experimentRepository.GetByFarmAsync(farmId)).ToList();
        var areas = _context.Areas.Where(a => a.FarmId == farmId).ToList();
        var experimentIds = experiments.Select(e => e.Id).ToList();
        var batches = _context.Batches.Where(b => experimentIds.Contains(b.ExperimentId)).ToList();
        var alerts = await _alertRepository.GetActiveAlertsAsync();
        var farmAlerts = alerts.Where(a => experimentIds.Contains(a.ExperimentId ?? Guid.Empty)).ToList();

        var healthScore = 100.0;
        if (farmAlerts.Any(a => a.Severity == AlertSeverity.Critical))
            healthScore -= 30;
        else if (farmAlerts.Any(a => a.Severity == AlertSeverity.High))
            healthScore -= 15;
        if (farmAlerts.Count > 0)
            healthScore -= Math.Min(20, farmAlerts.Count * 2);

        var status = healthScore >= 80 ? "Healthy" : healthScore >= 60 ? "Warning" : "Critical";

        return new FarmHealthDto
        {
            FarmId = farm.Id,
            FarmCode = farm.FarmCode,
            FarmName = farm.FarmName,
            Location = farm.Location,
            TotalAreas = areas.Count,
            TotalExperiments = experiments.Count,
            ActiveExperiments = experiments.Count(e => e.Status == ExperimentStatus.Active),
            TotalBatches = batches.Count,
            ActiveAlerts = farmAlerts.Count,
            CriticalAlerts = farmAlerts.Count(a => a.Severity == AlertSeverity.Critical),
            HealthScore = Math.Max(0, healthScore),
            Status = status
        };
    }

    public async Task<List<LatestSensorReadingDto>> GetLatestSensorReadingsAsync(Guid? farmId = null, Guid? experimentId = null)
    {
        var sensors = await _sensorRepository.GetAllAsync();

        var result = new List<LatestSensorReadingDto>();
        var thresholds = _context.SensorThresholdRules.Where(r => r.IsActive).ToList();

        foreach (var sensor in sensors)
        {
            var latestReading = await _sensorRepository.GetLatestReadingBySensorAsync(sensor.Id);
            var threshold = thresholds.FirstOrDefault(t => t.BatchId.HasValue || t.ExperimentId == (experimentId ?? Guid.Empty));

            var status = "Normal";
            if (latestReading != null && threshold != null)
            {
                if (latestReading.Value < (threshold.MinValue ?? 0) || latestReading.Value > (threshold.MaxValue ?? decimal.MaxValue))
                    status = "Alert";
            }

            result.Add(new LatestSensorReadingDto
            {
                SensorId = sensor.Id,
                SensorCode = sensor.SensorCode,
                SensorType = sensor.SensorType.ToString(),
                LatestValue = latestReading?.Value ?? 0,
                Unit = latestReading?.Unit,
                LastRecordedAt = latestReading?.RecordedAt ?? DateTime.MinValue,
                MinThreshold = threshold?.MinValue,
                MaxThreshold = threshold?.MaxValue,
                Status = status
            });
        }

        return result;
    }

    public async Task<List<SensorReadingDto>> GetSensorHistoryAsync(Guid sensorId, DateTime? fromDate = null, DateTime? toDate = null, int limit = 100)
    {
        var sensor = await _sensorRepository.GetByIdAsync(sensorId);
        if (sensor == null) return new List<SensorReadingDto>();

        var data = await _sensorRepository.GetSensorDataAsync(sensorId, fromDate, toDate, limit);

        return data.Select(d => new SensorReadingDto
        {
            Id = d.Id,
            SensorId = d.SensorId,
            SensorCode = sensor.SensorCode,
            SensorType = sensor.SensorType.ToString(),
            Value = d.Value,
            Unit = d.Unit,
            RecordedAt = d.RecordedAt,
            ExperimentId = d.ExperimentId,
            BatchId = d.BatchId
        }).ToList();
    }

    public async Task<List<AlertSummaryDto>> GetActiveAlertsAsync(Guid? experimentId = null)
    {
        var alerts = await _alertRepository.GetActiveAlertsAsync();
        if (experimentId.HasValue)
            alerts = alerts.Where(a => a.ExperimentId == experimentId.Value).ToList();

        var result = new List<AlertSummaryDto>();
        foreach (var alert in alerts)
        {
            var experiment = alert.ExperimentId.HasValue ? await _experimentRepository.GetByIdAsync(alert.ExperimentId.Value) : null;
            result.Add(new AlertSummaryDto
            {
                AlertId = alert.Id,
                Title = alert.Title,
                Message = alert.Message,
                Severity = alert.Severity.ToString(),
                IsResolved = alert.IsResolved,
                CreatedAt = alert.CreatedAt,
                ExperimentId = alert.ExperimentId,
                ExperimentCode = experiment?.ExperimentCode,
                SensorId = alert.SensorId,
                SensorCode = alert.Sensor?.SensorCode,
                BatchId = alert.BatchId,
                BatchCode = alert.Batch?.BatchCode
            });
        }

        return result;
    }

    public async Task<List<ExperimentStatusDto>> GetExperimentStatusesAsync(Guid? farmId = null)
    {
        var experiments = farmId.HasValue
            ? await _experimentRepository.GetByFarmAsync(farmId.Value)
            : await _experimentRepository.GetAllAsync();

        var result = new List<ExperimentStatusDto>();
        foreach (var exp in experiments)
        {
            var stages = await _stageRepository.GetByExperimentAsync(exp.Id);
            var batches = _context.Batches.Where(b => b.ExperimentId == exp.Id).ToList();
            var tasks = await _taskRepository.GetByExperimentAsync(exp.Id);
            var alerts = await _alertRepository.GetActiveAlertsByExperimentAsync(exp.Id);

            var completedStages = stages.Count(s => !string.IsNullOrEmpty(s.ResultSummary));
            var progress = stages.Count > 0 ? (completedStages * 100.0 / stages.Count) : 0;

            result.Add(new ExperimentStatusDto
            {
                ExperimentId = exp.Id,
                ExperimentCode = exp.ExperimentCode,
                Title = exp.Title,
                Status = exp.Status.ToString(),
                StartDate = exp.StartDate,
                EndDate = exp.EndDate,
                FarmId = exp.FarmId,
                FarmName = exp.Farm?.FarmName,
                ResearcherId = exp.ResearcherId,
                ResearcherName = exp.Researcher?.FullName,
                TotalStages = stages.Count,
                CompletedStages = completedStages,
                TotalBatches = batches.Count,
                ActiveBatches = batches.Count(b => b.Status == BatchStatus.Growing),
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == Model.Enums.TaskStatus.Completed),
                ActiveAlerts = alerts.Count,
                ProgressPercentage = Math.Round(progress, 2)
            });
        }

        return result;
    }

    // ========== T25: KPIs and Personnel Performance ==========

    public async Task<DashboardKpiDto> GetKpisAsync(Guid? farmId = null, Guid? experimentId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var experiments = farmId.HasValue
            ? await _experimentRepository.GetByFarmAsync(farmId.Value)
            : await _experimentRepository.GetAllAsync();

        if (experimentId.HasValue)
            experiments = experiments.Where(e => e.Id == experimentId.Value).ToList();

        var experimentIds = experiments.Select(e => e.Id).ToList();
        var tasks = await _taskRepository.GetAllAsync();
        var filteredTasks = tasks.Where(t => experimentIds.Contains(t.ExperimentId)).ToList();
        var batches = _context.Batches.Where(b => experimentIds.Contains(b.ExperimentId)).ToList();
        var measurements = _context.MeasurementRecords.Where(m => experimentIds.Contains(m.ExperimentId)).ToList();

        if (fromDate.HasValue)
        {
            filteredTasks = filteredTasks.Where(t => t.CreatedAt >= fromDate.Value).ToList();
            measurements = measurements.Where(m => m.MeasuredAt >= fromDate.Value).ToList();
        }

        if (toDate.HasValue)
        {
            filteredTasks = filteredTasks.Where(t => t.CreatedAt <= toDate.Value).ToList();
            measurements = measurements.Where(m => m.MeasuredAt <= toDate.Value).ToList();
        }

        var completedTasks = filteredTasks.Where(t => t.Status == Model.Enums.TaskStatus.Completed).ToList();
        var onTimeCompleted = completedTasks.Count(t => t.DueDate.HasValue && t.UpdatedAt <= t.DueDate.Value);

        var dailyMetrics = filteredTasks
            .GroupBy(t => DateOnly.FromDateTime(t.UpdatedAt.Date))
            .Select(g => new DailyMetricDto
            {
                Date = g.Key,
                TasksCompleted = g.Count(t => t.Status == Model.Enums.TaskStatus.Completed),
                TasksCreated = g.Count(),
                MeasurementsRecorded = measurements.Count(m => DateOnly.FromDateTime(m.MeasuredAt.Date) == g.Key)
            })
            .OrderByDescending(d => d.Date)
            .Take(30)
            .ToList();

        return new DashboardKpiDto
        {
            FarmId = farmId,
            ExperimentId = experimentId,
            FromDate = fromDate ?? DateTime.UtcNow.AddDays(-30),
            ToDate = toDate ?? DateTime.UtcNow,
            TotalTasks = filteredTasks.Count,
            CompletedTasks = completedTasks.Count,
            PendingTasks = filteredTasks.Count(t => t.Status == Model.Enums.TaskStatus.Pending),
            OverdueTasks = filteredTasks.Count(t => t.Status == Model.Enums.TaskStatus.Overdue),
            InProgressTasks = filteredTasks.Count(t => t.Status == Model.Enums.TaskStatus.InProgress),
            TaskCompletionRate = filteredTasks.Count > 0 ? Math.Round(completedTasks.Count * 100.0 / filteredTasks.Count, 2) : 0,
            OnTimeCompletionRate = completedTasks.Count > 0 ? Math.Round(onTimeCompleted * 100.0 / completedTasks.Count, 2) : 0,
            TotalExperiments = experiments.Count,
            ActiveExperiments = experiments.Count(e => e.Status == ExperimentStatus.Active),
            CompletedExperiments = experiments.Count(e => e.Status == ExperimentStatus.Completed),
            TotalBatches = batches.Count,
            ActiveBatches = batches.Count(b => b.Status == BatchStatus.Growing),
            HarvestedBatches = batches.Count(b => b.Status == BatchStatus.Harvested),
            TotalMeasurementRecords = measurements.Count,
            DailyCompletions = dailyMetrics
        };
    }

    public async Task<List<PersonnelPerformanceDto>> GetPersonnelPerformanceAsync(Guid? farmId = null, Guid? experimentId = null)
    {
        var experiments = farmId.HasValue
            ? await _experimentRepository.GetByFarmAsync(farmId.Value)
            : await _experimentRepository.GetAllAsync();

        if (experimentId.HasValue)
            experiments = experiments.Where(e => e.Id == experimentId.Value).ToList();

        var experimentIds = experiments.Select(e => e.Id).ToList();
        var tasks = await _taskRepository.GetAllAsync();
        var filteredTasks = tasks.Where(t => experimentIds.Contains(t.ExperimentId) && t.AssignedTo.HasValue).ToList();

        var userIds = filteredTasks.Select(t => t.AssignedTo!.Value).Distinct().ToList();
        var result = new List<PersonnelPerformanceDto>();

        foreach (var userId in userIds)
        {
            var performance = await GetPersonnelPerformanceByIdAsync(userId);
            if (performance != null)
                result.Add(performance);
        }

        return result.OrderByDescending(p => p.CompletionRate).ToList();
    }

    public async Task<PersonnelPerformanceDto?> GetPersonnelPerformanceByIdAsync(Guid userId)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return null;

        var assignments = await _assignmentRepository.GetByAssigneeAsync(userId);
        var tasks = assignments.Select(a => a.Task).Where(t => t != null).ToList();

        var completedTasks = tasks.Where(t => t.Status == Model.Enums.TaskStatus.Completed).ToList();
        var onTimeCompleted = completedTasks.Count(t => t.DueDate.HasValue && t.UpdatedAt <= t.DueDate.Value);

        var avgCompletionDays = completedTasks.Count > 0
            ? completedTasks.Average(t => (t.UpdatedAt - t.CreatedAt).TotalDays)
            : 0;

        var taskTypeBreakdown = tasks
            .GroupBy(t => t.Type)
            .Select(g => new TaskTypePerformanceDto
            {
                TaskType = g.Key.ToString(),
                TotalAssigned = g.Count(),
                Completed = g.Count(t => t.Status == Model.Enums.TaskStatus.Completed),
                CompletionRate = g.Count() > 0 ? Math.Round(g.Count(t => t.Status == Model.Enums.TaskStatus.Completed) * 100.0 / g.Count(), 2) : 0
            })
            .ToList();

        var roleName = user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "Unknown";

        return new PersonnelPerformanceDto
        {
            UserId = userId,
            FullName = user.FullName,
            Email = user.Email,
            Role = roleName,
            TotalTasksAssigned = tasks.Count,
            TasksCompleted = completedTasks.Count,
            TasksInProgress = tasks.Count(t => t.Status == Model.Enums.TaskStatus.InProgress),
            TasksOverdue = tasks.Count(t => t.Status == Model.Enums.TaskStatus.Overdue),
            TasksPending = tasks.Count(t => t.Status == Model.Enums.TaskStatus.Pending),
            CompletionRate = tasks.Count > 0 ? Math.Round(completedTasks.Count * 100.0 / tasks.Count, 2) : 0,
            OnTimeCompletionRate = completedTasks.Count > 0 ? Math.Round(onTimeCompleted * 100.0 / completedTasks.Count, 2) : 0,
            AverageCompletionDays = Math.Round(avgCompletionDays, 2),
            LastActivityAt = completedTasks.MaxBy(t => t.UpdatedAt)?.UpdatedAt ?? user.UpdatedAt,
            TaskTypeBreakdown = taskTypeBreakdown
        };
    }

    public async Task<List<ExperimentProgressDto>> GetExperimentProgressAsync(Guid? farmId = null)
    {
        var experiments = farmId.HasValue
            ? await _experimentRepository.GetByFarmAsync(farmId.Value)
            : await _experimentRepository.GetAllAsync();

        var result = new List<ExperimentProgressDto>();
        foreach (var exp in experiments)
        {
            var progress = await GetExperimentProgressByIdAsync(exp.Id);
            if (progress != null)
                result.Add(progress);
        }

        return result;
    }

    public async Task<ExperimentProgressDto?> GetExperimentProgressByIdAsync(Guid experimentId)
    {
        var experiment = await _experimentRepository.GetByIdAsync(experimentId);
        if (experiment == null) return null;

        var stages = await _stageRepository.GetByExperimentAsync(experimentId);
        var groups = _context.ExperimentGroups.Where(g => g.ExperimentId == experimentId).ToList();
        var batches = _context.Batches.Where(b => b.ExperimentId == experimentId).ToList();
        var tasks = await _taskRepository.GetByExperimentAsync(experimentId);

        var stageProgress = stages.Select(s =>
        {
            var stageTasks = tasks.Where(t => t.ExperimentStageId == s.Id).ToList();
            var completedStageTasks = stageTasks.Count(t => t.Status == Model.Enums.TaskStatus.Completed);
            var isActive = s.StartDate.HasValue && s.EndDate.HasValue &&
                          DateOnly.FromDateTime(DateTime.UtcNow) >= s.StartDate.Value &&
                          DateOnly.FromDateTime(DateTime.UtcNow) <= s.EndDate.Value;

            return new StageProgressDto
            {
                StageId = s.Id,
                StageName = s.StageName,
                StageType = s.StageType.ToString(),
                StageOrder = s.StageOrder,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsActive = isActive,
                TotalTasks = stageTasks.Count,
                CompletedTasks = completedStageTasks,
                ProgressPercentage = stageTasks.Count > 0 ? Math.Round(completedStageTasks * 100.0 / stageTasks.Count, 2) : 0
            };
        }).ToList();

        var groupProgress = groups.Select(g =>
        {
            var groupBatches = batches.Where(b => b.GroupId == g.Id).ToList();
            var batchIds = groupBatches.Select(b => b.Id).ToList();
            var measurements = _context.MeasurementRecords.Where(m => batchIds.Contains(m.BatchId)).ToList();
            var measurementValues = measurements.Where(m => m.Value.HasValue).Select(m => m.Value!.Value).ToList();
            var avgValue = measurementValues.Any() ? measurementValues.Average() : (decimal?)null;

            var measurementDef = _context.MeasurementDefinitions.FirstOrDefault(md => md.GroupId == g.Id);

            return new GroupProgressDto
            {
                GroupId = g.Id,
                GroupName = g.GroupName,
                GroupType = g.GroupType.ToString(),
                TreatmentDescription = g.TreatmentDescription,
                TotalBatches = groupBatches.Count,
                TotalMeasurementRecords = measurements.Count,
                AverageMetricValue = avgValue,
                MetricName = measurementDef?.MetricName
            };
        }).ToList();

        var totalDays = 0;
        var currentDay = 0;
        if (experiment.StartDate.HasValue && experiment.EndDate.HasValue)
        {
            totalDays = experiment.EndDate.Value.DayNumber - experiment.StartDate.Value.DayNumber;
            currentDay = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - experiment.StartDate.Value.DayNumber;
        }

        var overallProgress = stageProgress.Count > 0 ? stageProgress.Average(s => s.ProgressPercentage) : 0;

        return new ExperimentProgressDto
        {
            ExperimentId = experimentId,
            ExperimentCode = experiment.ExperimentCode,
            Title = experiment.Title,
            Status = experiment.Status.ToString(),
            StartDate = experiment.StartDate,
            EndDate = experiment.EndDate,
            CurrentDay = Math.Max(0, currentDay),
            TotalDays = totalDays,
            OverallProgress = Math.Round(overallProgress, 2),
            StageProgress = stageProgress,
            GroupProgress = groupProgress
        };
    }
}
