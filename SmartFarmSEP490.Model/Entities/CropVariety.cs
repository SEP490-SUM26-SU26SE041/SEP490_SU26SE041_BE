using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class CropVariety
{
    public Guid Id { get; set; }

    public Guid CropId { get; set; }

    public string VarietyName { get; set; } = null!;

    public string? Origin { get; set; }

    public int? GrowthDurationDays { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

    public virtual Crop Crop { get; set; } = null!;

    public virtual ICollection<ExperimentRequest> ExperimentRequests { get; set; } = new List<ExperimentRequest>();

    public virtual ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();

    public virtual ICollection<KnowledgeDocument> KnowledgeDocuments { get; set; } = new List<KnowledgeDocument>();

    public virtual ICollection<ProcedureTemplate> ProcedureTemplates { get; set; } = new List<ProcedureTemplate>();
}
