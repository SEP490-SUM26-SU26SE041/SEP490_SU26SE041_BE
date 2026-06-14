using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class TaskSkillRequirement
{
    public Guid TaskId { get; set; }

    public Guid SkillId { get; set; }

    public int RequiredLevel { get; set; }

    public virtual Skill Skill { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
