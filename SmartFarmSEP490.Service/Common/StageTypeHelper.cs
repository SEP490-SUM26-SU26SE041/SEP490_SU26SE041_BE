using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Service.Common;

public static class StageTypeHelper
{
    public static ExperimentStageType Parse(string? value) => ExperimentStageTypeExtensions.Parse(value);
}
