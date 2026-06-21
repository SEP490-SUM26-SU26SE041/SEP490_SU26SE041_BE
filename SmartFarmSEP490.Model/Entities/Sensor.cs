using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

public partial class Sensor
{
    public Guid Id { get; set; }

    public string SensorCode { get; set; } = null!;

    public string? Description { get; set; }

    public SensorType SensorType { get; set; } = SensorType.Other;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual ICollection<SensorDatum> SensorData { get; set; } = new List<SensorDatum>();
}
