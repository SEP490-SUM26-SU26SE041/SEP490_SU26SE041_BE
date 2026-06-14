using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class UserSkill
{
    public Guid UserId { get; set; }

    public Guid SkillId { get; set; }

    public int ProficiencyLevel { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Skill Skill { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
