using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class ProcedureTemplate
{
    public Guid Id { get; set; }

    public Guid? CropVarietyId { get; set; }

    public string TemplateName { get; set; } = null!;

    public string? Objective { get; set; }

    public string? Description { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual CropVariety? CropVariety { get; set; }

    public virtual ICollection<ExperimentRequest> ExperimentRequests { get; set; } = new List<ExperimentRequest>();

    public virtual ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();

    public virtual ICollection<ProcedureTemplateStep> ProcedureTemplateSteps { get; set; } = new List<ProcedureTemplateStep>();
}
