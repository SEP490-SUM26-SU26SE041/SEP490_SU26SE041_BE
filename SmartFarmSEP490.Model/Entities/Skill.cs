using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class Skill
{
    public Guid Id { get; set; }

    public string SkillName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TaskSkillRequirement> TaskSkillRequirements { get; set; } = new List<TaskSkillRequirement>();

    public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
}
