using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class RequestReview
{
    public Guid Id { get; set; }

    public Guid RequestId { get; set; }

    public Guid ReviewerId { get; set; }

    public string? Comment { get; set; }

    public DateTime ReviewedAt { get; set; }

    public virtual ExperimentRequest Request { get; set; } = null!;

    public virtual User Reviewer { get; set; } = null!;
}
