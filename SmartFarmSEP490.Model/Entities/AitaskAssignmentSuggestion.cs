using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

public partial class AitaskAssignmentSuggestion
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public Guid SuggestedUserId { get; set; }

    public decimal? MatchScore { get; set; }

    public string? Reason { get; set; }

    public AIReviewStatus ReviewStatus { get; set; } = AIReviewStatus.Suggested;

    public Guid? ReviewedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? ReviewedByNavigation { get; set; }

    public virtual User SuggestedUser { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
