using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

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

    public ExperimentStageType StageType { get; set; } = ExperimentStageType.Other;

    public virtual ProcedureTemplate Template { get; set; } = null!;
}
