using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class PlantHealthAssessment
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public Guid? BatchId { get; set; }

    public Guid? ImageId { get; set; }

    public Guid? AssessedBy { get; set; }

    public string? AimodelName { get; set; }

    public decimal? Aiconfidence { get; set; }

    public string? Aisuggestion { get; set; }

    public string? HumanConclusion { get; set; }

    public string? AssessmentData { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? AssessedByNavigation { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual PlantImage? Image { get; set; }
}
