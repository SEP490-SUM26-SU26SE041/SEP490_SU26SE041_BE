namespace SmartFarmSEP490.Model.Enums;

public static class ExperimentStageTypeExtensions
{
    public static ExperimentStageType Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return ExperimentStageType.Other;
        var s = value.Trim();
        if (int.TryParse(s, out var n) && Enum.IsDefined(typeof(ExperimentStageType), n))
            return (ExperimentStageType)n;
        return Enum.TryParse<ExperimentStageType>(s, ignoreCase: true, out var r)
            ? r
            : ExperimentStageType.Other;
    }
}
