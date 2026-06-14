using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class Crop
{
    public Guid Id { get; set; }

    public string CropName { get; set; } = null!;

    public string? ScientificName { get; set; }

    public string? Category { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<CropVariety> CropVarieties { get; set; } = new List<CropVariety>();
}
