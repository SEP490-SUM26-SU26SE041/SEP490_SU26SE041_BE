namespace SmartFarmSEP490.Model.DTOs;

// ============ Experiment DTOs ============

public class CreateExperimentDto
{
    public Guid? RequestId { get; set; }
    public Guid FarmId { get; set; }
    public Guid? CropVarietyId { get; set; }
    public Guid? ProcedureTemplateId { get; set; }
    public string ExperimentCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string? Hypothesis { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class UpdateExperimentDto
{
    public string? ExperimentCode { get; set; }
    public string? Title { get; set; }
    public string? Objective { get; set; }
    public string? Hypothesis { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Status { get; set; }
}

public class ExperimentResponseDto
{
    public Guid Id { get; set; }
    public string ExperimentCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string? Hypothesis { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? RequestId { get; set; }
    public Guid FarmId { get; set; }
    public string? FarmName { get; set; }
    public Guid ResearcherId { get; set; }
    public string? ResearcherName { get; set; }
    public Guid? CropVarietyId { get; set; }
    public string? CropVarietyName { get; set; }
    public Guid? ProcedureTemplateId { get; set; }
    public string? ProcedureTemplateName { get; set; }
    public List<ExperimentStageResponseDto> Stages { get; set; } = new();
    public List<ExperimentGroupResponseDto> Groups { get; set; } = new();
    public List<MeasurementDefinitionResponseDto> MeasurementDefinitions { get; set; } = new();
    public ExperimentDesignResponseDto? Design { get; set; }
}

// ============ ExperimentStage DTOs ============

public class CreateExperimentStageDto
{
    public string StageName { get; set; } = string.Empty;
    public int StageOrder { get; set; }
    public string? Objective { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class UpdateExperimentStageDto
{
    public string? StageName { get; set; }
    public int? StageOrder { get; set; }
    public string? Objective { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? ResultSummary { get; set; }
    public string? ResultData { get; set; }
}

public class ExperimentStageResponseDto
{
    public Guid Id { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int StageOrder { get; set; }
    public string? Objective { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? ResultSummary { get; set; }
    public string? ResultData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ============ ExperimentGroup DTOs ============

public class CreateExperimentGroupDto
{
    public string GroupName { get; set; } = string.Empty;
    public string? TreatmentDescription { get; set; }
}

public class UpdateExperimentGroupDto
{
    public string? GroupName { get; set; }
    public string? TreatmentDescription { get; set; }
}

public class ExperimentGroupResponseDto
{
    public Guid Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? TreatmentDescription { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============ ExperimentDesign DTOs ============

public class CreateExperimentDesignDto
{
    public int? ReplicationCount { get; set; }
    public string? RandomizationMethod { get; set; }
    public string? DesignParameters { get; set; }
}

public class UpdateExperimentDesignDto
{
    public int? ReplicationCount { get; set; }
    public string? RandomizationMethod { get; set; }
    public string? DesignParameters { get; set; }
}

public class ExperimentDesignResponseDto
{
    public Guid Id { get; set; }
    public int? ReplicationCount { get; set; }
    public string? RandomizationMethod { get; set; }
    public string? DesignParameters { get; set; }
}

// ============ MeasurementDefinition DTOs ============

public class CreateMeasurementDefinitionDto
{
    public Guid? GroupId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }
    public string? Description { get; set; }
}

public class UpdateMeasurementDefinitionDto
{
    public Guid? GroupId { get; set; }
    public string? MetricName { get; set; }
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }
    public string? Description { get; set; }
}

public class MeasurementDefinitionResponseDto
{
    public Guid Id { get; set; }
    public Guid? GroupId { get; set; }
    public string? GroupName { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }
    public string? Description { get; set; }
}

// ============ ProcedureTemplate DTOs ============

public class CreateProcedureTemplateDto
{
    public Guid? CropVarietyId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? Objective { get; set; }
    public string? Description { get; set; }
    public List<CreateProcedureTemplateStepDto> Steps { get; set; } = new();
}

public class CreateProcedureTemplateStepDto
{
    public int StepOrder { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public int? ExpectedDurationDays { get; set; }
    public string? RequiredSkillDescription { get; set; }
}

public class ProcedureTemplateResponseDto
{
    public Guid Id { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? Objective { get; set; }
    public string? Description { get; set; }
    public Guid? CropVarietyId { get; set; }
    public string? CropVarietyName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProcedureTemplateStepResponseDto> Steps { get; set; } = new();
}

public class ProcedureTemplateStepResponseDto
{
    public Guid Id { get; set; }
    public int StepOrder { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public int? ExpectedDurationDays { get; set; }
    public string? RequiredSkillDescription { get; set; }
}

// ============ CareSchedule DTOs ============

public class CreateCareScheduleDto
{
    public Guid? ExperimentStageId { get; set; }
    public Guid? BatchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instruction { get; set; }
    public int? FrequencyDays { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class UpdateCareScheduleDto
{
    public Guid? ExperimentStageId { get; set; }
    public Guid? BatchId { get; set; }
    public string? Title { get; set; }
    public string? Instruction { get; set; }
    public int? FrequencyDays { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class CareScheduleResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instruction { get; set; }
    public int? FrequencyDays { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid ExperimentId { get; set; }
    public Guid? ExperimentStageId { get; set; }
    public string? ExperimentStageName { get; set; }
    public Guid? BatchId { get; set; }
    public string? BatchCode { get; set; }
}
