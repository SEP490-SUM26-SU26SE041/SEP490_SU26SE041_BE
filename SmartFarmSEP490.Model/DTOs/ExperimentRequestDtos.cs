namespace SmartFarmSEP490.Model.DTOs;

// ============ ExperimentRequest DTOs ============

public class CreateExperimentRequestDto
{
    public Guid FarmId { get; set; }
    public Guid? CropVarietyId { get; set; }
    public Guid? ProcedureTemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public DateOnly? ExpectedStartDate { get; set; }
    public DateOnly? ExpectedEndDate { get; set; }
    public string? MonitoringPlan { get; set; }
}

public class UpdateExperimentRequestDto
{
    public Guid? CropVarietyId { get; set; }
    public Guid? ProcedureTemplateId { get; set; }
    public string? Title { get; set; }
    public string? Objective { get; set; }
    public DateOnly? ExpectedStartDate { get; set; }
    public DateOnly? ExpectedEndDate { get; set; }
    public string? MonitoringPlan { get; set; }
}

public class ReviewExperimentRequestDto
{
    public string Result { get; set; } = string.Empty; // "Approved" or "Rejected"
    public string? Comment { get; set; }
}

public class ExperimentRequestResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly? ExpectedStartDate { get; set; }
    public DateOnly? ExpectedEndDate { get; set; }
    public string? MonitoringPlan { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid FarmId { get; set; }
    public string? FarmName { get; set; }
    public Guid ResearcherId { get; set; }
    public string? ResearcherName { get; set; }
    public Guid? CropVarietyId { get; set; }
    public string? CropVarietyName { get; set; }
    public Guid? ProcedureTemplateId { get; set; }
    public string? ProcedureTemplateName { get; set; }
    public List<RequestReviewResponseDto> Reviews { get; set; } = new();
}

public class RequestReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
    public string? Comment { get; set; }
    public string Result { get; set; } = string.Empty;
    public DateTime ReviewedAt { get; set; }
}
