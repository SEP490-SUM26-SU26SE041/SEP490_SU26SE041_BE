using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.ExperimentStages;
using SmartFarmSEP490.Service.Interfaces.Tasks;
using SmartFarmSEP490.Service.Services.Helpers;

namespace SmartFarmSEP490.Service.Services.Tasks;

public class MeasurementStatisticsService : IMeasurementStatisticsService
{
    private readonly SmartFarmDbContext _context;
    private readonly IExperimentStageRepository _stageRepository;

    public MeasurementStatisticsService(
        SmartFarmDbContext context,
        IExperimentStageRepository stageRepository)
    {
        _context = context;
        _stageRepository = stageRepository;
    }

    public async Task<StageStatisticsResponseDto> GetStageStatisticsAsync(
        Guid stageId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? groupId = null)
    {
        var stage = await _stageRepository.GetByIdAsync(stageId)
            ?? throw new KeyNotFoundException("Không tìm thấy giai đoạn.");

        var definitions = await GetDefinitionsForStageAsync(stage, includeAllForStage: false);

        var groups = await GetGroupsAsync(stage.ExperimentId, groupId);

        var batchLookup = await GetBatchLookupAsync(stage.ExperimentId);
        var validBatchIds = batchLookup.Keys.ToHashSet();

        var records = await LoadRecordsAsync(stage.ExperimentId, stageId, fromDate, toDate, validBatchIds);

        var response = new StageStatisticsResponseDto
        {
            StageId = stage.Id,
            StageName = stage.StageName,
            ExperimentId = stage.ExperimentId,
            StatisticsType = "MeasurementStats",
            DefinitionCount = definitions.Count,
            GeneratedAt = DateTime.UtcNow,
            Groups = new List<GroupStatisticsDto>()
        };

        var groupValuesByMetric = new Dictionary<Guid, Dictionary<Guid, List<decimal>>>();

        foreach (var group in groups)
        {
            var groupBatchIds = batchLookup
                .Where(kv => kv.Value.groupId == group.Id)
                .Select(kv => kv.Key)
                .ToHashSet();

            var groupRecords = records
                .Where(r => groupBatchIds.Contains(r.BatchId))
                .ToList();

            var stats = new GroupStatisticsDto
            {
                GroupId = group.Id,
                GroupName = group.GroupName,
                GroupType = group.GroupType.ToString(),
                BatchCount = groupBatchIds.Count,
                TotalSamples = groupRecords.Count,
                Metrics = new List<MetricStatisticDto>(),
                GrowthOverTime = new List<MetricGrowthPointDto>()
            };

            var growthBuckets = new Dictionary<DateTime, (decimal Sum, int Count)>();

            foreach (var def in definitions)
            {
                var values = groupRecords
                    .Where(r => r.MeasurementDefinitionId == def.Id && r.Value.HasValue)
                    .Select(r => r.Value!.Value)
                    .ToList();

                if (!groupValuesByMetric.ContainsKey(def.Id))
                    groupValuesByMetric[def.Id] = new Dictionary<Guid, List<decimal>>();

                groupValuesByMetric[def.Id][group.Id] = values;

                var metric = BuildMetricStatistic(def, values);
                stats.Metrics.Add(metric);
            }

            BuildGrowthOverTime(groupRecords, definitions, stats.GrowthOverTime!);
            response.Groups.Add(stats);
        }

        response.CrossGroupComparison = BuildCrossGroupComparison(definitions, groupValuesByMetric, response.Groups);
        return response;
    }

    public async Task<StageStatisticsResponseDto> GetExperimentOverallStatisticsAsync(
        Guid experimentId,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var experiment = await _context.Experiments.FindAsync(experimentId)
            ?? throw new KeyNotFoundException("Không tìm thấy thực nghiệm.");

        var definitions = await _context.MeasurementDefinitions
            .Where(d => d.ExperimentId == experimentId)
            .ToListAsync();

        var groups = await GetGroupsAsync(experimentId, null);
        var batchLookup = await GetBatchLookupAsync(experimentId);
        var validBatchIds = batchLookup.Keys.ToHashSet();

        var records = await LoadRecordsAsync(experimentId, stageId: null, fromDate, toDate, validBatchIds);

        var response = new StageStatisticsResponseDto
        {
            StageId = Guid.Empty,
            StageName = "Tổng hợp toàn thực nghiệm",
            ExperimentId = experimentId,
            StatisticsType = "OverallEvaluation",
            DefinitionCount = definitions.Count,
            GeneratedAt = DateTime.UtcNow,
            Groups = new List<GroupStatisticsDto>()
        };

        var groupValuesByMetric = new Dictionary<Guid, Dictionary<Guid, List<decimal>>>();

        foreach (var group in groups)
        {
            var groupBatchIds = batchLookup
                .Where(kv => kv.Value.groupId == group.Id)
                .Select(kv => kv.Key)
                .ToHashSet();

            var groupRecords = records.Where(r => groupBatchIds.Contains(r.BatchId)).ToList();

            var stats = new GroupStatisticsDto
            {
                GroupId = group.Id,
                GroupName = group.GroupName,
                GroupType = group.GroupType.ToString(),
                BatchCount = groupBatchIds.Count,
                TotalSamples = groupRecords.Count,
                Metrics = new List<MetricStatisticDto>(),
                GrowthOverTime = new List<MetricGrowthPointDto>()
            };

            foreach (var def in definitions)
            {
                var values = groupRecords
                    .Where(r => r.MeasurementDefinitionId == def.Id && r.Value.HasValue)
                    .Select(r => r.Value!.Value)
                    .ToList();

                if (!groupValuesByMetric.ContainsKey(def.Id))
                    groupValuesByMetric[def.Id] = new Dictionary<Guid, List<decimal>>();

                groupValuesByMetric[def.Id][group.Id] = values;

                stats.Metrics.Add(BuildMetricStatistic(def, values));
            }

            BuildGrowthOverTime(groupRecords, definitions, stats.GrowthOverTime!);
            response.Groups.Add(stats);
        }

        response.CrossGroupComparison = BuildCrossGroupComparison(definitions, groupValuesByMetric, response.Groups);
        return response;
    }

    public async Task<byte[]> ExportStageStatisticsAsync(StageStatisticsExportRequestDto request)
    {
        var stats = await GetStageStatisticsAsync(request.StageId);
        var format = (request.Format ?? "csv").Trim().ToLowerInvariant();

        return format switch
        {
            "xlsx" => BuildXlsx(stats),
            "csv" => BuildCsv(stats),
            _ => throw new ArgumentException("Format không hợp lệ. Chỉ hỗ trợ 'csv' hoặc 'xlsx'.")
        };
    }

    public async Task<List<string>> ValidateMeasurementValueAsync(
        Guid measurementDefinitionId,
        decimal value)
    {
        var def = await _context.MeasurementDefinitions.FindAsync(measurementDefinitionId);
        if (def == null) return new List<string> { $"Không tìm thấy chỉ số đo lường với Id={measurementDefinitionId}" };

        var errors = new List<string>();
        var unit = (def.Unit ?? string.Empty).Trim().ToLowerInvariant();
        var metricName = (def.MetricName ?? string.Empty).Trim().ToLowerInvariant();

        if (value < 0)
        {
            errors.Add($"Giá trị {def.MetricName} không được âm.");
            return errors;
        }

        if (unit == "%" && value > 100)
            errors.Add($"Giá trị {def.MetricName} là phần trăm nên phải nằm trong [0, 100].");

        if (metricName.Contains("màu sắc lá") && (value < 1 || value > 5))
            errors.Add($"Màu sắc lá theo thang điểm 1-5, giá trị {value} không hợp lệ.");

        if (def.TargetValue.HasValue && def.TargetValue.Value > 0)
        {
            var ratio = value / def.TargetValue.Value;
            if (ratio > 5m)
                errors.Add($"Giá trị {value} vượt quá 5 lần target {def.TargetValue.Value}. Vui lòng kiểm tra lại.");
        }

        return errors;
    }

    // ============== Private helpers ==============

    private async Task<List<MeasurementDefinition>> GetDefinitionsForStageAsync(ExperimentStage stage, bool includeAllForStage)
    {
        if (stage.StageType == ExperimentStageType.Evaluation || includeAllForStage)
        {
            return await _context.MeasurementDefinitions
                .Where(d => d.ExperimentId == stage.ExperimentId)
                .OrderBy(d => d.MetricName)
                .ToListAsync();
        }

        var measurementDefinitionIds = await _context.MeasurementRecords
            .Where(r => r.ExperimentStageId == stage.Id && r.MeasurementDefinitionId.HasValue)
            .Select(r => r.MeasurementDefinitionId!.Value)
            .Distinct()
            .ToListAsync();

        return await _context.MeasurementDefinitions
            .Where(d => measurementDefinitionIds.Contains(d.Id) || d.ExperimentId == stage.ExperimentId)
            .OrderBy(d => d.MetricName)
            .ToListAsync();
    }

    private async Task<List<ExperimentGroup>> GetGroupsAsync(Guid experimentId, Guid? groupId)
    {
        var query = _context.ExperimentGroups.Where(g => g.ExperimentId == experimentId);
        if (groupId.HasValue) query = query.Where(g => g.Id == groupId.Value);
        return await query.OrderBy(g => g.GroupName).ToListAsync();
    }

    private async Task<Dictionary<Guid, (Guid groupId, string batchCode)>> GetBatchLookupAsync(Guid experimentId)
    {
        var batches = await _context.Batches
            .Where(b => b.ExperimentId == experimentId && b.DeletedAt == null && b.GroupId != null)
            .Select(b => new { b.Id, b.GroupId, b.BatchCode })
            .ToListAsync();

        return batches.ToDictionary(b => b.Id, b => (b.GroupId!.Value, b.BatchCode));
    }

    private async Task<List<MeasurementRecord>> LoadRecordsAsync(
        Guid experimentId,
        Guid? stageId,
        DateTime? fromDate,
        DateTime? toDate,
        HashSet<Guid> validBatchIds)
    {
        var query = _context.MeasurementRecords
            .Where(r => r.ExperimentId == experimentId && r.DeletedAt == null && r.MeasurementDefinitionId != null);

        if (stageId.HasValue) query = query.Where(r => r.ExperimentStageId == stageId.Value);
        if (fromDate.HasValue) query = query.Where(r => r.MeasuredAt >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(r => r.MeasuredAt <= toDate.Value);

        var records = await query
            .Select(r => new
            {
                r.Id,
                r.BatchId,
                r.MeasurementDefinitionId,
                r.Value,
                r.MeasuredAt
            })
            .ToListAsync();

        return records
            .Where(r => r.MeasurementDefinitionId.HasValue && validBatchIds.Contains(r.BatchId))
            .Select(r => new MeasurementRecord
            {
                Id = r.Id,
                BatchId = r.BatchId,
                MeasurementDefinitionId = r.MeasurementDefinitionId,
                Value = r.Value,
                MeasuredAt = r.MeasuredAt
            })
            .ToList();
    }

    private static MetricStatisticDto BuildMetricStatistic(MeasurementDefinition def, List<decimal> values)
    {
        if (values.Count == 0)
        {
            return new MetricStatisticDto
            {
                DefinitionId = def.Id,
                MetricName = def.MetricName,
                Unit = def.Unit,
                TargetValue = def.TargetValue,
                SampleCount = 0,
                Average = 0,
                Min = 0,
                Max = 0,
                StdDev = 0,
                Median = 0,
                Q1 = 0,
                Q3 = 0,
                ReachesTarget = false,
                TargetAchievementRatio = 0
            };
        }

        var avg = StatisticsHelper.Average(values);
        var targetRatio = def.TargetValue.HasValue && def.TargetValue.Value > 0
            ? Math.Round(avg / def.TargetValue.Value, 4)
            : 0m;

        return new MetricStatisticDto
        {
            DefinitionId = def.Id,
            MetricName = def.MetricName,
            Unit = def.Unit,
            TargetValue = def.TargetValue,
            SampleCount = values.Count,
            Average = avg,
            Min = values.Min(),
            Max = values.Max(),
            StdDev = StatisticsHelper.StdDev(values),
            Median = StatisticsHelper.Median(values),
            Q1 = StatisticsHelper.Quartile(values, 0.25),
            Q3 = StatisticsHelper.Quartile(values, 0.75),
            ReachesTarget = def.TargetValue.HasValue && avg >= def.TargetValue.Value,
            TargetAchievementRatio = targetRatio
        };
    }

    private static void BuildGrowthOverTime(
        List<MeasurementRecord> groupRecords,
        List<MeasurementDefinition> definitions,
        List<MetricGrowthPointDto> output)
    {
        if (groupRecords.Count == 0) return;

        var distinctDates = groupRecords
            .Select(r => r.MeasuredAt.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        decimal previousAvg = 0;
        bool hasPrevious = false;

        foreach (var date in distinctDates)
        {
            var sameDay = groupRecords.Where(r => r.MeasuredAt.Date == date).ToList();
            var dayValues = sameDay
                .Where(r => r.Value.HasValue)
                .Select(r => r.Value!.Value)
                .ToList();

            if (dayValues.Count == 0) continue;

            var avg = StatisticsHelper.Average(dayValues);
            var growth = hasPrevious ? StatisticsHelper.GrowthRate(avg, previousAvg) : 0;

            output.Add(new MetricGrowthPointDto
            {
                MeasuredAt = date,
                Average = avg,
                SampleCount = dayValues.Count,
                GrowthRatePercent = growth
            });

            previousAvg = avg;
            hasPrevious = true;
        }
    }

    private static CrossGroupComparisonDto? BuildCrossGroupComparison(
        List<MeasurementDefinition> definitions,
        Dictionary<Guid, Dictionary<Guid, List<decimal>>> groupValuesByMetric,
        List<GroupStatisticsDto> groupStats)
    {
        if (groupStats.Count < 2 || definitions.Count == 0) return null;

        var comparison = new CrossGroupComparisonDto
        {
            Metrics = new List<MetricCrossGroupComparisonDto>()
        };

        var perMetricBest = new Dictionary<Guid, Guid>();

        foreach (var def in definitions)
        {
            if (!groupValuesByMetric.TryGetValue(def.Id, out var groupMap)) continue;

            var metricComparison = new MetricCrossGroupComparisonDto
            {
                DefinitionId = def.Id,
                MetricName = def.MetricName,
                Unit = def.Unit,
                GroupValues = new List<GroupMetricComparisonDto>()
            };

            decimal maxAvg = decimal.MinValue;
            Guid bestGroupId = Guid.Empty;

            foreach (var (groupId, values) in groupMap)
            {
                var avg = StatisticsHelper.Average(values);
                metricComparison.GroupValues.Add(new GroupMetricComparisonDto
                {
                    GroupId = groupId,
                    GroupName = groupStats.FirstOrDefault(g => g.GroupId == groupId)?.GroupName ?? "",
                    Average = avg,
                    SampleCount = values.Count,
                    StdDev = StatisticsHelper.StdDev(values)
                });

                if (values.Count > 0 && avg > maxAvg)
                {
                    maxAvg = avg;
                    bestGroupId = groupId;
                }
            }

            var first = groupMap.Values.FirstOrDefault(v => v.Count > 0);
            if (first != null && groupMap.Count >= 2)
            {
                var orderedGroups = groupMap.Where(kv => kv.Value.Count > 0).ToList();
                if (orderedGroups.Count >= 2)
                {
                    var valuesA = orderedGroups[0].Value;
                    var valuesB = orderedGroups[1].Value;
                    metricComparison.SignificantDifference = StatisticsHelper.IsSignificantDifference(valuesA, valuesB);
                }
            }

            if (metricComparison.GroupValues.Count >= 2)
            {
                metricComparison.MaxDifference = metricComparison.GroupValues.Max(g => g.Average) -
                                                metricComparison.GroupValues.Min(g => g.Average);
            }

            metricComparison.BestGroupId = bestGroupId;
            metricComparison.BestGroupName = metricComparison.GroupValues
                .FirstOrDefault(g => g.GroupId == bestGroupId)?.GroupName;

            perMetricBest[def.Id] = bestGroupId;
            comparison.Metrics.Add(metricComparison);
        }

        var bestGroupVotes = perMetricBest.Values
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (bestGroupVotes != null)
        {
            comparison.BestGroupId = bestGroupVotes.Key;
            comparison.BestGroupName = groupStats.FirstOrDefault(g => g.GroupId == bestGroupVotes.Key)?.GroupName;
            comparison.Summary = $"Nhóm {comparison.BestGroupName} đạt kết quả tốt nhất ở {bestGroupVotes.Count()}/{comparison.Metrics.Count} chỉ số.";
        }

        return comparison;
    }

    // ============== Export builders ==============

    private static byte[] BuildCsv(StageStatisticsResponseDto stats)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Báo cáo thống kê giai đoạn: {stats.StageName}");
        sb.AppendLine($"# Thực nghiệm: {stats.ExperimentId}");
        sb.AppendLine($"# Sinh lúc: {stats.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        sb.AppendLine("GroupId,GroupName,GroupType,BatchCount,MetricName,Unit,TargetValue,SampleCount,Average,Min,Max,StdDev,Median,Q1,Q3,ReachesTarget,AchievementRatio");
        foreach (var g in stats.Groups)
        {
            foreach (var m in g.Metrics)
            {
                sb.Append(Csv(g.GroupId.ToString())).Append(',')
                  .Append(Csv(g.GroupName)).Append(',')
                  .Append(Csv(g.GroupType)).Append(',')
                  .Append(g.BatchCount).Append(',')
                  .Append(Csv(m.MetricName)).Append(',')
                  .Append(Csv(m.Unit)).Append(',')
                  .Append(m.TargetValue?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',')
                  .Append(m.SampleCount).Append(',')
                  .Append(m.Average.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(m.Min.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(m.Max.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(m.StdDev.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(m.Median?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',')
                  .Append(m.Q1?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',')
                  .Append(m.Q3?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',')
                  .Append(m.ReachesTarget).Append(',')
                  .Append(m.TargetAchievementRatio.ToString(CultureInfo.InvariantCulture))
                  .AppendLine();
            }
        }

        if (stats.CrossGroupComparison != null && stats.CrossGroupComparison.Metrics.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("# So sánh giữa các nhóm");
            sb.AppendLine("MetricName,Unit,GroupName,Average,SampleCount,StdDev");
            foreach (var m in stats.CrossGroupComparison.Metrics)
            {
                foreach (var gv in m.GroupValues)
                {
                    sb.Append(Csv(m.MetricName)).Append(',')
                      .Append(Csv(m.Unit)).Append(',')
                      .Append(Csv(gv.GroupName)).Append(',')
                      .Append(gv.Average.ToString(CultureInfo.InvariantCulture)).Append(',')
                      .Append(gv.SampleCount).Append(',')
                      .Append(gv.StdDev.ToString(CultureInfo.InvariantCulture))
                      .AppendLine();
                }
            }
        }

        var anyGrowth = stats.Groups.Any(g => g.GrowthOverTime != null && g.GrowthOverTime.Count > 0);
        if (anyGrowth)
        {
            sb.AppendLine();
            sb.AppendLine("# Tăng trưởng theo thời gian");
            sb.AppendLine("GroupName,Date,Average,SampleCount,GrowthRatePercent");
            foreach (var g in stats.Groups)
            {
                if (g.GrowthOverTime == null) continue;
                foreach (var p in g.GrowthOverTime)
                {
                    sb.Append(Csv(g.GroupName)).Append(',')
                      .Append(p.MeasuredAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append(',')
                      .Append(p.Average.ToString(CultureInfo.InvariantCulture)).Append(',')
                      .Append(p.SampleCount).Append(',')
                      .Append(p.GrowthRatePercent.ToString(CultureInfo.InvariantCulture))
                      .AppendLine();
                }
            }
        }

        // Prepend UTF-8 BOM để Excel nhận diện đúng encoding tiếng Việt
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[bom.Length + body.Length];
        Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
        Buffer.BlockCopy(body, 0, result, bom.Length, body.Length);
        return result;
    }

    private static byte[] BuildXlsx(StageStatisticsResponseDto stats)
    {
        // Tạo file .xlsx tối thiểu theo chuẩn SpreadsheetML 2003 (XML-based, mở được bằng Excel).
        // Đây là định dạng "XML Spreadsheet 2003" với mime "application/vnd.ms-excel",
        // tuy không phải OOXML chuẩn nhưng hỗ trợ tiếng Việt UTF-8 và đủ cho researcher xem/sắp xếp.
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
        sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
        sb.AppendLine("<Styles><Style ss:ID=\"Header\"><Font ss:Bold=\"1\"/><Interior ss:Color=\"#DDDDDD\" ss:Pattern=\"Solid\"/></Style></Styles>");

        // Sheet 1: Thống kê theo nhóm
        sb.AppendLine("<Worksheet ss:Name=\"Thống kê\"><Table>");
        sb.AppendLine("<Row ss:StyleID=\"Header\">");
        AppendCell(sb, "GroupId", true);
        AppendCell(sb, "GroupName", true);
        AppendCell(sb, "GroupType", true);
        AppendCell(sb, "BatchCount", true);
        AppendCell(sb, "MetricName", true);
        AppendCell(sb, "Unit", true);
        AppendCell(sb, "Target", true);
        AppendCell(sb, "Samples", true);
        AppendCell(sb, "Average", true);
        AppendCell(sb, "Min", true);
        AppendCell(sb, "Max", true);
        AppendCell(sb, "StdDev", true);
        AppendCell(sb, "Median", true);
        AppendCell(sb, "Q1", true);
        AppendCell(sb, "Q3", true);
        AppendCell(sb, "ReachesTarget", true);
        AppendCell(sb, "Achievement", true);
        sb.AppendLine("</Row>");

        foreach (var g in stats.Groups)
        {
            foreach (var m in g.Metrics)
            {
                sb.Append("<Row>");
                AppendCell(sb, g.GroupId.ToString());
                AppendCell(sb, g.GroupName);
                AppendCell(sb, g.GroupType);
                AppendNumberCell(sb, g.BatchCount);
                AppendCell(sb, m.MetricName);
                AppendCell(sb, m.Unit);
                AppendNumberCell(sb, m.TargetValue);
                AppendNumberCell(sb, m.SampleCount);
                AppendNumberCell(sb, m.Average);
                AppendNumberCell(sb, m.Min);
                AppendNumberCell(sb, m.Max);
                AppendNumberCell(sb, m.StdDev);
                AppendNumberCell(sb, m.Median);
                AppendNumberCell(sb, m.Q1);
                AppendNumberCell(sb, m.Q3);
                AppendCell(sb, m.ReachesTarget ? "Có" : "Không");
                AppendNumberCell(sb, m.TargetAchievementRatio);
                sb.AppendLine("</Row>");
            }
        }
        sb.AppendLine("</Table></Worksheet>");

        // Sheet 2: So sánh giữa các nhóm
        if (stats.CrossGroupComparison != null && stats.CrossGroupComparison.Metrics.Count > 0)
        {
            sb.AppendLine("<Worksheet ss:Name=\"So sánh nhóm\"><Table>");
            sb.AppendLine("<Row ss:StyleID=\"Header\">");
            AppendCell(sb, "MetricName", true);
            AppendCell(sb, "Unit", true);
            AppendCell(sb, "GroupName", true);
            AppendCell(sb, "Average", true);
            AppendCell(sb, "Samples", true);
            AppendCell(sb, "StdDev", true);
            AppendCell(sb, "BestGroup", true);
            AppendCell(sb, "SignificantDifference", true);
            sb.AppendLine("</Row>");

            foreach (var m in stats.CrossGroupComparison.Metrics)
            {
                foreach (var gv in m.GroupValues)
                {
                    sb.Append("<Row>");
                    AppendCell(sb, m.MetricName);
                    AppendCell(sb, m.Unit);
                    AppendCell(sb, gv.GroupName);
                    AppendNumberCell(sb, gv.Average);
                    AppendNumberCell(sb, gv.SampleCount);
                    AppendNumberCell(sb, gv.StdDev);
                    AppendCell(sb, m.BestGroupName);
                    AppendCell(sb, m.SignificantDifference ? "Có" : "Không");
                    sb.AppendLine("</Row>");
                }
            }
            sb.AppendLine("</Table></Worksheet>");
        }

        // Sheet 3: Tăng trưởng theo thời gian
        var anyGrowth = stats.Groups.Any(g => g.GrowthOverTime != null && g.GrowthOverTime.Count > 0);
        if (anyGrowth)
        {
            sb.AppendLine("<Worksheet ss:Name=\"Tăng trưởng\"><Table>");
            sb.AppendLine("<Row ss:StyleID=\"Header\">");
            AppendCell(sb, "GroupName", true);
            AppendCell(sb, "Date", true);
            AppendCell(sb, "Average", true);
            AppendCell(sb, "Samples", true);
            AppendCell(sb, "GrowthRate%", true);
            sb.AppendLine("</Row>");

            foreach (var g in stats.Groups)
            {
                if (g.GrowthOverTime == null) continue;
                foreach (var p in g.GrowthOverTime)
                {
                    sb.Append("<Row>");
                    AppendCell(sb, g.GroupName);
                    AppendCell(sb, p.MeasuredAt.ToString("yyyy-MM-dd"));
                    AppendNumberCell(sb, p.Average);
                    AppendNumberCell(sb, p.SampleCount);
                    AppendNumberCell(sb, p.GrowthRatePercent);
                    sb.AppendLine("</Row>");
                }
            }
            sb.AppendLine("</Table></Worksheet>");
        }

        sb.AppendLine("</Workbook>");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static void AppendCell(StringBuilder sb, string? value, bool isHeader = false)
    {
        var style = isHeader ? " ss:StyleID=\"Header\"" : string.Empty;
        sb.Append("<Cell").Append(style).Append("><Data ss:Type=\"String\">")
          .Append(EscapeXml(value ?? string.Empty))
          .Append("</Data></Cell>");
    }

    private static void AppendNumberCell(StringBuilder sb, decimal? value)
    {
        if (!value.HasValue)
        {
            sb.Append("<Cell><Data ss:Type=\"String\"></Data></Cell>");
            return;
        }
        sb.Append("<Cell><Data ss:Type=\"Number\">")
          .Append(value.Value.ToString(CultureInfo.InvariantCulture))
          .Append("</Data></Cell>");
    }

    private static void AppendNumberCell(StringBuilder sb, int value)
    {
        sb.Append("<Cell><Data ss:Type=\"Number\">").Append(value).Append("</Data></Cell>");
    }

    private static string EscapeXml(string s) => System.Security.SecurityElement.Escape(s) ?? string.Empty;

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var needsQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n');
        if (!needsQuote) return value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
