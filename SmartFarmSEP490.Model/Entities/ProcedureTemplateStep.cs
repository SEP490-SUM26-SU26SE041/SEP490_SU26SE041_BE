using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class ProcedureTemplateStep
{
    public Guid Id { get; set; }

    public Guid TemplateId { get; set; }

    public int StepOrder { get; set; }

    public string Title { get; set; } = null!;

    public string Instruction { get; set; } = null!;

    public int? ExpectedDurationDays { get; set; }

    public string? RequiredSkillDescription { get; set; }

    public virtual ProcedureTemplate Template { get; set; } = null!;
}
