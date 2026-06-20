namespace SmartFarmSEP490.Model.DTOs;

using SmartFarmSEP490.Model.Enums;

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
    public ReviewResult Result { get; set; } = ReviewResult.Approved;
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
    public ReviewerInfoDto? Reviewer { get; set; }
    public string? Comment { get; set; }
    public string Result { get; set; } = string.Empty;
    public DateTime ReviewedAt { get; set; }
}

public class ReviewerInfoDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ProfileDescription { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<string>? Roles { get; set; }
}

public class FarmResourceSummaryDto
{
    public Guid FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public int TotalBeds { get; set; }
    public int AvailableBeds { get; set; }
    public int InUseBeds { get; set; }
    public int MaintenanceBeds { get; set; }
    public int TotalSensors { get; set; }
    public int TotalAreas { get; set; }
}

public class ResourceValidationResultDto
{
    public bool IsValid { get; set; }
    public bool SufficientBeds { get; set; }
    public bool SufficientSensors { get; set; }
    public string? Message { get; set; }
    public FarmResourceSummaryDto? Resources { get; set; }
}
