using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class ExperimentRequest
{
    public Guid Id { get; set; }

    public Guid FarmId { get; set; }

    public Guid ResearcherId { get; set; }

    public Guid? CropVarietyId { get; set; }

    public Guid? ProcedureTemplateId { get; set; }

    public string Title { get; set; } = null!;

    public string Objective { get; set; } = null!;

    public DateOnly? ExpectedStartDate { get; set; }

    public DateOnly? ExpectedEndDate { get; set; }

    public string? MonitoringPlan { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Status { get; set; } = "Pending";

    public virtual CropVariety? CropVariety { get; set; }

    public virtual ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();

    public virtual Farm Farm { get; set; } = null!;

    public virtual ProcedureTemplate? ProcedureTemplate { get; set; }

    public virtual ICollection<RequestReview> RequestReviews { get; set; } = new List<RequestReview>();

    public virtual User Researcher { get; set; } = null!;
}
