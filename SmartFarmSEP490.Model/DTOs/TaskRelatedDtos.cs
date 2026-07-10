namespace SmartFarmSEP490.Model.DTOs;

// ============ Researcher-created Tasks Filter ============

public class TaskFilterScope
{
    public const string Overdue = "overdue";
    public const string Today = "today";
    public const string Upcoming = "upcoming";
}

public class ResearcherCreatedTaskFilterDto
{
    public Guid? CreatorId { get; set; }
    public Guid? ExperimentId { get; set; }
    public string? Scope { get; set; }
    public int? UpcomingDays { get; set; }
}

public class GenerateTaskFromScheduleDto
{
    public Guid ScheduleId { get; set; }
    public DateOnly? TargetDate { get; set; }
    public Guid? BatchId { get; set; }
    public DateTime? DueDate { get; set; }
}

public class GeneratedTaskResultDto
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public Guid? BatchId { get; set; }
    public string? BatchCode { get; set; }
    public Guid ScheduleId { get; set; }
    public bool IsNew { get; set; }
    public string? Message { get; set; }
}

public class GenerateByStageResultDto
{
    public Guid StageId { get; set; }
    public string? StageName { get; set; }
    public DateOnly? StageStartDate { get; set; }
    public DateOnly? StageEndDate { get; set; }
    public int TotalSchedules { get; set; }
    public int TasksGenerated { get; set; }
    public int TasksSkipped { get; set; }
    public int ExistingTasksCount { get; set; }
    public bool HasError { get; set; }
    public string? Message { get; set; }
    public List<GeneratedTaskResultDto> Tasks { get; set; } = new();
}

public class GenerateByExperimentResultDto
{
    public Guid ExperimentId { get; set; }
    public int TotalStages { get; set; }
    public int TotalSchedules { get; set; }
    public int TasksGenerated { get; set; }
    public int TasksSkipped { get; set; }
    public int StagesSkipped { get; set; }
    public bool HasError { get; set; }
    public string? Message { get; set; }
    public List<GenerateByStageResultDto> StageResults { get; set; } = new();
    public List<GeneratedTaskResultDto> Tasks { get; set; } = new();
}

// ============ Task Report DTOs ============

public class CreateTaskReportDto
{
    public Guid TaskId { get; set; }
    public string? ReportText { get; set; }
    public Dictionary<string, object>? ResultData { get; set; }
}

public class UpdateTaskReportDto
{
    public string? ReportText { get; set; }
    public Dictionary<string, object>? ResultData { get; set; }
}

public class TaskReportResponseDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public Guid ReporterId { get; set; }
    public string? ReporterName { get; set; }
    public string? ReportText { get; set; }
    public object? ResultData { get; set; }
    public DateTime ReportedAt { get; set; }
    public List<PlantImageResponseDto> Images { get; set; } = new();
}

// ============ Measurement Record DTOs ============

public class CreateMeasurementRecordDto
{
    public Guid ExperimentId { get; set; }
    public Guid? ExperimentStageId { get; set; }
    public Guid BatchId { get; set; }
    public Guid? MeasurementDefinitionId { get; set; }
    public decimal? Value { get; set; }
    public string? TextValue { get; set; }
    public Dictionary<string, object>? ExtraData { get; set; }
    public DateTime? MeasuredAt { get; set; }
}

public class UpdateMeasurementRecordDto
{
    public decimal? Value { get; set; }
    public string? TextValue { get; set; }
    public Dictionary<string, object>? ExtraData { get; set; }
    public DateTime? MeasuredAt { get; set; }
}

public class MeasurementRecordResponseDto
{
    public Guid Id { get; set; }
    public Guid ExperimentId { get; set; }
    public string? ExperimentTitle { get; set; }
    public Guid? ExperimentStageId { get; set; }
    public string? ExperimentStageName { get; set; }
    public Guid BatchId { get; set; }
    public string? BatchCode { get; set; }
    public Guid? MeasurementDefinitionId { get; set; }
    public string? MetricName { get; set; }
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? Value { get; set; }
    public string? TextValue { get; set; }
    public object? ExtraData { get; set; }
    public Guid? MeasuredBy { get; set; }
    public string? MeasuredByName { get; set; }
    public DateTime MeasuredAt { get; set; }
}

// ============ Plant Image / Task Image DTOs ============

public class UploadTaskImageDto
{
    public Guid ExperimentId { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? TaskReportId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime? CapturedAt { get; set; }
}

public class PlantImageResponseDto
{
    public Guid Id { get; set; }
    public Guid ExperimentId { get; set; }
    public Guid? BatchId { get; set; }
    public string? BatchCode { get; set; }
    public Guid? TaskReportId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public Guid? UploadedBy { get; set; }
    public string? UploadedByName { get; set; }
    public DateTime? CapturedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
