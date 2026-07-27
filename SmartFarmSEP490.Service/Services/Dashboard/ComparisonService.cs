using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Batches;
using SmartFarmSEP490.Repository.Interfaces.ExperimentGroups;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Repository.Interfaces.MeasurementDefinitions;
using SmartFarmSEP490.Service.Interfaces.Dashboard;

namespace SmartFarmSEP490.Service.Services.Dashboard;

public class ComparisonService : IComparisonService
{
    private readonly SmartFarmDbContext _context;
    private readonly IExperimentRepository _experimentRepository;
    private readonly IExperimentGroupRepository _groupRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly IMeasurementDefinitionRepository _measurementDefRepository;

    public ComparisonService(
        SmartFarmDbContext context,
        IExperimentRepository experimentRepository,
        IExperimentGroupRepository groupRepository,
        IBatchRepository batchRepository,
        IMeasurementDefinitionRepository measurementDefRepository)
    {
        _context = context;
        _experimentRepository = experimentRepository;
        _groupRepository = groupRepository;
        _batchRepository = batchRepository;
        _measurementDefRepository = measurementDefRepository;
    }

    public async Task<CultivationComparisonDto?> GetComparisonAsync(Guid experimentId)
    {
        var experiment = await _experimentRepository.GetByIdAsync(experimentId);
        if (experiment == null) return null;

        var groups = (await _groupRepository.GetByExperimentAsync(experimentId)).ToList();
        var batches = _context.Batches.Where(b => b.ExperimentId == experimentId).ToList();
        var design = _context.ExperimentDesigns.FirstOrDefault(d => d.ExperimentId == experimentId);
        var measurementDefs = _context.MeasurementDefinitions.Where(m => m.ExperimentId == experimentId).ToList();

        var groupComparisons = new List<GroupComparisonDto>();

        foreach (var group in groups)
        {
            var groupBatches = batches.Where(b => b.GroupId == group.Id).ToList();
            var batchIds = groupBatches.Select(b => b.Id).ToList();
            var measurements = _context.MeasurementRecords.Where(m => batchIds.Contains(m.BatchId)).ToList();
            var groupMetricDefs = measurementDefs.Where(m => m.GroupId == group.Id).ToList();

            var metricComparisons = new List<MetricComparisonDto>();

            foreach (var metricDef in groupMetricDefs)
            {
                var metricMeasurements = measurements
                    .Where(m => m.MeasurementDefinitionId == metricDef.Id && m.Value.HasValue)
                    .Select(m => m.Value!.Value)
                    .ToList();

                if (metricMeasurements.Count == 0) continue;

                var avg = metricMeasurements.Average();
                var min = metricMeasurements.Min();
                var max = metricMeasurements.Max();
                var variance = metricMeasurements.Sum(v => (v - avg) * (v - avg)) / metricMeasurements.Count;
                var stdDev = (decimal)Math.Sqrt((double)variance);
                var targetVal = metricDef.TargetValue ?? 0;
                var withinTarget = targetVal != 0
                    ? metricMeasurements.Count(v => Math.Abs(v - targetVal) <= Math.Abs(targetVal) * 0.1m)
                    : 0;

                metricComparisons.Add(new MetricComparisonDto
                {
                    MetricName = metricDef.MetricName,
                    Unit = metricDef.Unit,
                    TargetValue = metricDef.TargetValue,
                    AverageValue = Math.Round(avg, 4),
                    MinValue = min,
                    MaxValue = max,
                    StandardDeviation = Math.Round(stdDev, 4),
                    Variance = Math.Round((decimal)variance, 4),
                    SampleSize = metricMeasurements.Count,
                    MeasurementsWithinTarget = withinTarget,
                    TargetAchievementRate = targetVal != 0 && metricMeasurements.Count > 0
                        ? Math.Round(withinTarget * 100.0 / metricMeasurements.Count, 2)
                        : 0
                });
            }

            var nonNullValues = measurements.Where(m => m.Value.HasValue).Select(m => m.Value!.Value).ToList();
            var statistics = CalculateStatistics(nonNullValues);

            var batchMetrics = groupBatches.Select(b =>
            {
                var batchMeasurements = measurements.Where(m => m.BatchId == b.Id).ToList();
                var batchMetricSeries = batchMeasurements
                    .OrderBy(m => m.MeasuredAt)
                    .Select(m => new MetricTimeSeriesDto
                    {
                        RecordedAt = m.MeasuredAt,
                        Value = m.Value ?? 0
                    }).ToList();

                var batchAvg = batchMeasurements.Where(m => m.Value.HasValue).Select(m => m.Value!.Value).DefaultIfEmpty(0).Average();

                return new BatchMetricDto
                {
                    BatchId = b.Id,
                    BatchCode = b.BatchCode,
                    Status = b.Status.ToString(),
                    PlantingDate = b.PlantingDate,
                    ExpectedHarvestDate = b.ExpectedHarvestDate,
                    PlantCount = b.PlantCount ?? 0,
                    AverageMetricValue = batchMeasurements.Any(m => m.Value.HasValue) ? Math.Round(batchAvg, 4) : null,
                    MeasurementCount = batchMeasurements.Count,
                    MetricTimeSeries = batchMetricSeries
                };
            }).ToList();

            groupComparisons.Add(new GroupComparisonDto
            {
                GroupId = group.Id,
                GroupName = group.GroupName,
                GroupType = group.GroupType.ToString(),
                TreatmentDescription = group.TreatmentDescription,
                TotalBatches = groupBatches.Count,
                TotalMeasurements = measurements.Count,
                MetricComparisons = metricComparisons,
                BatchMetrics = batchMetrics,
                Statistics = statistics
            });
        }

        var statisticalSummary = CalculateStatisticalSummary(groupComparisons);

        return new CultivationComparisonDto
        {
            ExperimentId = experimentId,
            ExperimentCode = experiment.ExperimentCode,
            Title = experiment.Title,
            Hypothesis = experiment.Hypothesis,
            DesignType = design?.DesignType.ToString() ?? "Not specified",
            ReplicationCount = design?.ReplicationCount ?? 0,
            StartDate = experiment.StartDate,
            EndDate = experiment.EndDate,
            GeneratedAt = DateTime.UtcNow,
            GroupComparisons = groupComparisons,
            StatisticalSummary = statisticalSummary
        };
    }

    public async Task<List<CultivationComparisonDto>> GetAllComparisonsAsync(Guid? farmId = null)
    {
        var experiments = farmId.HasValue
            ? await _experimentRepository.GetByFarmAsync(farmId.Value)
            : await _experimentRepository.GetAllAsync();

        var completedExperiments = experiments.Where(e => e.Status == ExperimentStatus.Completed).ToList();
        var result = new List<CultivationComparisonDto>();

        foreach (var exp in completedExperiments)
        {
            var comparison = await GetComparisonAsync(exp.Id);
            if (comparison != null)
                result.Add(comparison);
        }

        return result;
    }

    private MetricStatisticsDto CalculateStatistics(List<decimal> values)
    {
        if (values.Count == 0)
            return new MetricStatisticsDto { Count = 0 };

        var sorted = values.OrderBy(v => v).ToList();
        var mean = values.Average();
        var median = values.Count % 2 == 0
            ? (sorted[values.Count / 2 - 1] + sorted[values.Count / 2]) / 2
            : sorted[values.Count / 2];
        var variance = values.Sum(v => (v - mean) * (v - mean)) / values.Count;
        var stdDev = (decimal)Math.Sqrt((double)variance);
        var cv = mean != 0 ? (double)(stdDev / mean) * 100 : 0;

        var mode = values.GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;

        return new MetricStatisticsDto
        {
            Mean = Math.Round(mean, 4),
            Median = Math.Round(median, 4),
            Mode = mode,
            StandardDeviation = Math.Round(stdDev, 4),
            Variance = Math.Round(variance, 4),
            Min = sorted.First(),
            Max = sorted.Last(),
            Range = sorted.Last() - sorted.First(),
            Count = values.Count,
            CoefficientOfVariation = Math.Round(cv, 2)
        };
    }

    private StatisticalSummaryDto CalculateStatisticalSummary(List<GroupComparisonDto> groupComparisons)
    {
        var summary = new StatisticalSummaryDto();

        if (groupComparisons.Count < 2)
        {
            summary.Conclusion = "Insufficient groups for comparison";
            summary.KeyFindings.Add("Need at least 2 groups (Control and Treatment) for meaningful comparison");
            return summary;
        }

        var controlGroup = groupComparisons.FirstOrDefault(g => g.GroupType == "Control");
        var treatmentGroups = groupComparisons.Where(g => g.GroupType == "Treatment").ToList();

        if (controlGroup == null)
        {
            summary.Conclusion = "No control group found for comparison";
            summary.KeyFindings.Add("Experiment requires a control group for valid statistical analysis");
            return summary;
        }

        if (treatmentGroups.Count == 0)
        {
            summary.Conclusion = "No treatment groups found for comparison";
            summary.KeyFindings.Add("Experiment requires at least one treatment group");
            return summary;
        }

        var keyFindings = new List<string>();

        foreach (var treatment in treatmentGroups)
        {
            foreach (var controlMetric in controlGroup.MetricComparisons)
            {
                var treatmentMetric = treatment.MetricComparisons
                    .FirstOrDefault(m => m.MetricName == controlMetric.MetricName);

                if (treatmentMetric != null && controlMetric.AverageValue.HasValue && treatmentMetric.AverageValue.HasValue)
                {
                    var diff = treatmentMetric.AverageValue.Value - controlMetric.AverageValue.Value;
                    var percentDiff = controlMetric.AverageValue.Value != 0
                        ? (double)diff / (double)controlMetric.AverageValue.Value * 100
                        : 0;

                    var finding = $"Comparing {treatment.GroupName} to Control for {controlMetric.MetricName}: " +
                                  $"Treatment avg = {treatmentMetric.AverageValue:F2} vs Control avg = {controlMetric.AverageValue:F2} " +
                                  $"({(diff >= 0 ? "+" : "")}{percentDiff:F1}% difference)";

                    keyFindings.Add(finding);

                    if (treatmentMetric.TargetAchievementRate > controlMetric.TargetAchievementRate)
                    {
                        keyFindings.Add($"Treatment '{treatment.GroupName}' achieved target {treatmentMetric.TargetAchievementRate:F1}% vs Control {controlMetric.TargetAchievementRate:F1}% for {controlMetric.MetricName}");
                    }
                }
            }
        }

        summary.KeyFindings = keyFindings;
        summary.IsSignificant = keyFindings.Count > 0;
        summary.StatisticalTest = "Descriptive Statistics";
        summary.Conclusion = keyFindings.Count > 0
            ? "Treatment groups show measurable differences from control group"
            : "No significant differences observed between groups";

        summary.Recommendation = treatmentGroups.Any(t => t.MetricComparisons.Any(m => m.TargetAchievementRate > 70))
            ? "Consider scaling up treatments that achieved >70% target achievement rate"
            : "Continue monitoring; no treatment has achieved optimal target levels yet";

        return summary;
    }
}
