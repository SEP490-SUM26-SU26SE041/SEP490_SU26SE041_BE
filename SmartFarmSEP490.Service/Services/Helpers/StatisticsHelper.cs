using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartFarmSEP490.Service.Services.Helpers;

public static class StatisticsHelper
{
    public static decimal Average(IReadOnlyCollection<decimal> values)
    {
        if (values.Count == 0) return 0m;
        return Math.Round(values.Average(), 4);
    }

    public static decimal StdDev(IReadOnlyCollection<decimal> values)
    {
        if (values.Count <= 1) return 0m;
        var avg = (double)values.Average();
        var sumOfSquares = values.Sum(v => (double)v * (double)v);
        var variance = (sumOfSquares / values.Count) - (avg * avg);
        if (variance < 0) variance = 0;
        return Math.Round((decimal)Math.Sqrt(variance), 4);
    }

    public static decimal Median(IReadOnlyCollection<decimal> values)
    {
        if (values.Count == 0) return 0m;
        var sorted = values.OrderBy(v => v).ToList();
        var mid = sorted.Count / 2;
        if (sorted.Count % 2 == 0)
            return Math.Round((sorted[mid - 1] + sorted[mid]) / 2m, 4);
        return Math.Round(sorted[mid], 4);
    }

    public static decimal Quartile(IReadOnlyCollection<decimal> values, double percentile)
    {
        if (values.Count == 0) return 0m;
        var sorted = values.OrderBy(v => v).ToList();
        var pos = (sorted.Count - 1) * percentile;
        var lower = (int)Math.Floor(pos);
        var upper = (int)Math.Ceiling(pos);
        if (lower == upper) return Math.Round(sorted[lower], 4);
        var fraction = (decimal)(pos - lower);
        return Math.Round(sorted[lower] + (sorted[upper] - sorted[lower]) * fraction, 4);
    }

    public static decimal GrowthRate(decimal currentAvg, decimal previousAvg)
    {
        if (previousAvg == 0) return currentAvg == 0 ? 0 : 100m;
        return Math.Round((currentAvg - previousAvg) / Math.Abs(previousAvg) * 100m, 4);
    }

    public static bool IsSignificantDifference(
        IReadOnlyCollection<decimal> groupAValues,
        IReadOnlyCollection<decimal> groupBValues)
    {
        if (groupAValues.Count < 2 || groupBValues.Count < 2) return false;
        var avgA = (double)groupAValues.Average();
        var avgB = (double)groupBValues.Average();
        var varianceA = Variance(groupAValues);
        var varianceB = Variance(groupBValues);
        if (varianceA == 0 && varianceB == 0) return false;
        var pooledVariance = ((groupAValues.Count - 1) * varianceA + (groupBValues.Count - 1) * varianceB) /
                             (groupAValues.Count + groupBValues.Count - 2);
        if (pooledVariance <= 0) return false;
        var standardError = Math.Sqrt(pooledVariance * (1.0 / groupAValues.Count + 1.0 / groupBValues.Count));
        if (standardError == 0) return false;
        var tStat = Math.Abs(avgA - avgB) / standardError;
        return tStat > 1.96;
    }

    private static double Variance(IReadOnlyCollection<decimal> values)
    {
        if (values.Count < 2) return 0;
        var avg = (double)values.Average();
        return values.Sum(v => Math.Pow((double)v - avg, 2)) / (values.Count - 1);
    }
}
