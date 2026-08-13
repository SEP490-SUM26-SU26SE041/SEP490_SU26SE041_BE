using SmartFarmSEP490.Model.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartFarmSEP490.Service.Interfaces.Tasks;

public interface IMeasurementStatisticsService
{
    Task<StageStatisticsResponseDto> GetStageStatisticsAsync(
        Guid stageId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? groupId = null);

    Task<StageStatisticsResponseDto> GetExperimentOverallStatisticsAsync(
        Guid experimentId,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    Task<byte[]> ExportStageStatisticsAsync(StageStatisticsExportRequestDto request);

    Task<List<string>> ValidateMeasurementValueAsync(
        Guid measurementDefinitionId,
        decimal value);
}
