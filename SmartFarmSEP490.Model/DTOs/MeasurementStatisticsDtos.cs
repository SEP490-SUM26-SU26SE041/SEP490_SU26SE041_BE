using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model.DTOs;

// ============ Bulk Create Measurement Record ============

public class BulkCreateMeasurementRecordDto
{
    public Guid ExperimentId { get; set; }
    public Guid? ExperimentStageId { get; set; }
    public Guid BatchId { get; set; }
    public DateTime? MeasuredAt { get; set; }
    public Dictionary<string, object>? ExtraData { get; set; }
    public List<BulkMeasurementItemDto> Items { get; set; } = new();
}

public class BulkMeasurementItemDto
{
    public Guid MeasurementDefinitionId { get; set; }
    public decimal? Value { get; set; }
    public string? TextValue { get; set; }
}

public class BulkCreateMeasurementResultDto
{
    public Guid BatchId { get; set; }
    public Guid? ExperimentStageId { get; set; }
    public DateTime MeasuredAt { get; set; }
    public int Created { get; set; }
    public int Skipped { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<MeasurementRecordResponseDto> Records { get; set; } = new();
}

// ============ Stage Statistics ============

public class StageStatisticsResponseDto
{
    public Guid StageId { get; set; }
    public string? StageName { get; set; }
    public Guid ExperimentId { get; set; }
    public string StatisticsType { get; set; } = string.Empty;
    public int DefinitionCount { get; set; }
    public List<GroupStatisticsDto> Groups { get; set; } = new();
    public CrossGroupComparisonDto? CrossGroupComparison { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class GroupStatisticsDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string GroupType { get; set; } = string.Empty;
    public int BatchCount { get; set; }
    public int TotalSamples { get; set; }
    public List<MetricStatisticDto> Metrics { get; set; } = new();
    public List<MetricGrowthPointDto>? GrowthOverTime { get; set; }
}

public class MetricStatisticDto
{
    public Guid DefinitionId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }
    public int SampleCount { get; set; }
    public decimal Average { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal StdDev { get; set; }
    public decimal? Median { get; set; }
    public decimal? Q1 { get; set; }
    public decimal? Q3 { get; set; }
    public bool ReachesTarget { get; set; }
    public decimal TargetAchievementRatio { get; set; }
}

public class MetricGrowthPointDto
{
    public DateTime MeasuredAt { get; set; }
    public decimal Average { get; set; }
    public int SampleCount { get; set; }
    public decimal GrowthRatePercent { get; set; }
}

public class CrossGroupComparisonDto
{
    public List<MetricCrossGroupComparisonDto> Metrics { get; set; } = new();
    public Guid? BestGroupId { get; set; }
    public string? BestGroupName { get; set; }
    public string? Summary { get; set; }
}

public class MetricCrossGroupComparisonDto
{
    public Guid DefinitionId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public List<GroupMetricComparisonDto> GroupValues { get; set; } = new();
    public decimal MaxDifference { get; set; }
    public Guid? BestGroupId { get; set; }
    public string? BestGroupName { get; set; }
    public bool SignificantDifference { get; set; }
}

public class GroupMetricComparisonDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public decimal Average { get; set; }
    public int SampleCount { get; set; }
    public decimal StdDev { get; set; }
}

// ============ Export DTOs ============

public class StageStatisticsExportRequestDto
{
    public Guid StageId { get; set; }
    public string Format { get; set; } = "csv"; // "csv" or "xlsx"
    public bool IncludeRawRecords { get; set; } = false;
    public bool IncludeComparison { get; set; } = true;
}
