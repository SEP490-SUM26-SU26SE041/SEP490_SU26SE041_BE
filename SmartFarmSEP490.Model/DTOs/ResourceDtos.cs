using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model.DTOs;

// ============ Farm DTOs ============

public class CreateFarmDto
{
    public string FarmCode { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
}

public class UpdateFarmDto
{
    public string? FarmCode { get; set; }
    public string? FarmName { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
}

public class FarmResponseDto
{
    public Guid Id { get; set; }
    public string FarmCode { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AreaResponseDto> Areas { get; set; } = new();
}

// ============ Area DTOs ============

public class CreateAreaDto
{
    public Guid FarmId { get; set; }
    public string AreaCode { get; set; } = string.Empty;
    public string AreaName { get; set; } = string.Empty;
    public string? EnvironmentType { get; set; }
    public decimal? TotalArea { get; set; }
    public LocationStatus Status { get; set; } = LocationStatus.Available;
}

public class UpdateAreaDto
{
    public string? AreaCode { get; set; }
    public string? AreaName { get; set; }
    public string? EnvironmentType { get; set; }
    public decimal? TotalArea { get; set; }
    public LocationStatus? Status { get; set; }
}

public class AreaResponseDto
{
    public Guid Id { get; set; }
    public string AreaCode { get; set; } = string.Empty;
    public string AreaName { get; set; } = string.Empty;
    public string? EnvironmentType { get; set; }
    public decimal? TotalArea { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid FarmId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<BedResponseDto> Beds { get; set; } = new();
}

// ============ Bed DTOs ============

public class CreateBedDto
{
    public Guid AreaId { get; set; }
    public string BedCode { get; set; } = string.Empty;
    public string? SoilDescription { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
}

public class UpdateBedDto
{
    public string? BedCode { get; set; }
    public string? SoilDescription { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
}

public class BedResponseDto
{
    public Guid Id { get; set; }
    public string BedCode { get; set; } = string.Empty;
    public string? SoilDescription { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public string? AllocationStatus { get; set; }
    public Guid AreaId { get; set; }
    public string? AreaName { get; set; }
    public Guid FarmId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ============ Bed Reservation DTOs ============

public class ReserveBedsDto
{
    public Guid RequestId { get; set; }
    public List<Guid> BedIds { get; set; } = new();
}

public class BedReservationResponseDto
{
    public Guid RequestId { get; set; }
    public int ReservedCount { get; set; }
    public List<BedResponseDto> ReservedBeds { get; set; } = new();
}

// ============ ExperimentBedAssignment DTOs ============

public class CreateExperimentBedAssignmentDto
{
    public Guid? RequestId { get; set; }
    public Guid? ExperimentId { get; set; }
    public Guid BedId { get; set; }
    public DateOnly AssignedFrom { get; set; }
    public DateOnly? AssignedTo { get; set; }
    public string? Purpose { get; set; }
}

public class UpdateExperimentBedAssignmentDto
{
    public DateOnly? AssignedFrom { get; set; }
    public DateOnly? AssignedTo { get; set; }
    public string? Purpose { get; set; }
}

public class ExperimentBedAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid? RequestId { get; set; }
    public Guid? ExperimentId { get; set; }
    public string? ExperimentTitle { get; set; }
    public Guid BedId { get; set; }
    public string? BedCode { get; set; }
    public string? AllocationStatus { get; set; }
    public string? AreaName { get; set; }
    public string? FarmName { get; set; }
    public DateOnly AssignedFrom { get; set; }
    public DateOnly? AssignedTo { get; set; }
    public string? Purpose { get; set; }
}

// ============ Batch DTOs ============

public class CreateBatchDto
{
    public Guid ExperimentId { get; set; }
    public Guid? ExperimentBedAssignmentId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid? CropVarietyId { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public DateOnly? PlantingDate { get; set; }
    public DateOnly? ExpectedHarvestDate { get; set; }
    public int? PlantCount { get; set; }
    public string? Notes { get; set; }
}

public class UpdateBatchDto
{
    public Guid? ExperimentBedAssignmentId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid? CropVarietyId { get; set; }
    public string? BatchCode { get; set; }
    public DateOnly? PlantingDate { get; set; }
    public DateOnly? ExpectedHarvestDate { get; set; }
    public int? PlantCount { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
}

public class BatchResponseDto
{
    public Guid Id { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public DateOnly? PlantingDate { get; set; }
    public DateOnly? ExpectedHarvestDate { get; set; }
    public int? PlantCount { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid ExperimentId { get; set; }
    public string? ExperimentTitle { get; set; }
    public Guid? ExperimentBedAssignmentId { get; set; }
    public string? BedCode { get; set; }
    public string? AreaName { get; set; }
    public string? FarmName { get; set; }
    public Guid? GroupId { get; set; }
    public string? GroupName { get; set; }
    public Guid? CropVarietyId { get; set; }
    public string? CropVarietyName { get; set; }
}

// ============ Crop DTOs ============

public class CreateCropDto
{
    public string CropName { get; set; } = string.Empty;
    public string? ScientificName { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
}

public class CropResponseDto
{
    public Guid Id { get; set; }
    public string CropName { get; set; } = string.Empty;
    public string? ScientificName { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CropVarietyResponseDto> Varieties { get; set; } = new();
}

public class CreateCropVarietyDto
{
    public Guid CropId { get; set; }
    public string VarietyName { get; set; } = string.Empty;
    public string? Origin { get; set; }
    public int? GrowthDurationDays { get; set; }
    public string? Description { get; set; }
}

public class CropVarietyResponseDto
{
    public Guid Id { get; set; }
    public string VarietyName { get; set; } = string.Empty;
    public string? Origin { get; set; }
    public int? GrowthDurationDays { get; set; }
    public string? Description { get; set; }
    public Guid CropId { get; set; }
    public string? CropName { get; set; }
    public DateTime CreatedAt { get; set; }
}
