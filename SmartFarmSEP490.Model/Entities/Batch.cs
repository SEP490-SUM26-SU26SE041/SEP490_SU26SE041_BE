using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

public partial class Batch
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public Guid? ExperimentBedAssignmentId { get; set; }

    public Guid? GroupId { get; set; }

    public Guid? CropVarietyId { get; set; }

    public string BatchCode { get; set; } = null!;

    public DateOnly? PlantingDate { get; set; }

    public DateOnly? ExpectedHarvestDate { get; set; }

    public int? PlantCount { get; set; }

    public BatchStatus Status { get; set; } = BatchStatus.Planned;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual ICollection<CareSchedule> CareSchedules { get; set; } = new List<CareSchedule>();

    public virtual CropVariety? CropVariety { get; set; }

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ExperimentBedAssignment? ExperimentBedAssignment { get; set; }

    public virtual ExperimentGroup? Group { get; set; }

    public virtual ICollection<MeasurementRecord> MeasurementRecords { get; set; } = new List<MeasurementRecord>();

    public virtual ICollection<PlantHealthAssessment> PlantHealthAssessments { get; set; } = new List<PlantHealthAssessment>();

    public virtual ICollection<PlantImage> PlantImages { get; set; } = new List<PlantImage>();

    public virtual ICollection<SensorDatum> SensorData { get; set; } = new List<SensorDatum>();

    public virtual ICollection<SensorThresholdRule> SensorThresholdRules { get; set; } = new List<SensorThresholdRule>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
