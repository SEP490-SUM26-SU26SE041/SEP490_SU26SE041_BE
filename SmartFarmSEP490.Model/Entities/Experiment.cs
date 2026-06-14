using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class Experiment
{
    public Guid Id { get; set; }

    public Guid? RequestId { get; set; }

    public Guid FarmId { get; set; }

    public Guid ResearcherId { get; set; }

    public Guid? CropVarietyId { get; set; }

    public Guid? ProcedureTemplateId { get; set; }

    public string ExperimentCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Objective { get; set; } = null!;

    public string? Hypothesis { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

    public virtual ICollection<CareSchedule> CareSchedules { get; set; } = new List<CareSchedule>();

    public virtual CropVariety? CropVariety { get; set; }

    public virtual ICollection<ExperimentBedAssignment> ExperimentBedAssignments { get; set; } = new List<ExperimentBedAssignment>();

    public virtual ExperimentDesign? ExperimentDesign { get; set; }

    public virtual ICollection<ExperimentGroup> ExperimentGroups { get; set; } = new List<ExperimentGroup>();

    public virtual ICollection<ExperimentReport> ExperimentReports { get; set; } = new List<ExperimentReport>();

    public virtual ICollection<ExperimentStage> ExperimentStages { get; set; } = new List<ExperimentStage>();

    public virtual Farm Farm { get; set; } = null!;

    public virtual ICollection<MeasurementDefinition> MeasurementDefinitions { get; set; } = new List<MeasurementDefinition>();

    public virtual ICollection<MeasurementRecord> MeasurementRecords { get; set; } = new List<MeasurementRecord>();

    public virtual ICollection<PlantHealthAssessment> PlantHealthAssessments { get; set; } = new List<PlantHealthAssessment>();

    public virtual ICollection<PlantImage> PlantImages { get; set; } = new List<PlantImage>();

    public virtual ProcedureTemplate? ProcedureTemplate { get; set; }

    public virtual ExperimentRequest? Request { get; set; }

    public virtual User Researcher { get; set; } = null!;

    public virtual ICollection<SensorDatum> SensorData { get; set; } = new List<SensorDatum>();

    public virtual ICollection<SensorThresholdRule> SensorThresholdRules { get; set; } = new List<SensorThresholdRule>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
