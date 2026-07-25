using System.Text.Json;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.Service.Services.Tasks;

public class MeasurementRecordService : IMeasurementRecordService
{
    private readonly IMeasurementRecordRepository _recordRepository;
    private readonly IExperimentRepository _experimentRepository;

    private const int ClockSkewMinutes = 5;

    public MeasurementRecordService(
        IMeasurementRecordRepository recordRepository,
        IExperimentRepository experimentRepository)
    {
        _recordRepository = recordRepository;
        _experimentRepository = experimentRepository;
    }

    public async System.Threading.Tasks.Task<MeasurementRecordResponseDto> CreateAsync(CreateMeasurementRecordDto dto, Guid measuredBy)
    {
        var validationError = ValidateCreate(dto);
        if (validationError != null) throw new ArgumentException(validationError);

        var record = new MeasurementRecord
        {
            Id = Guid.NewGuid(),
            ExperimentId = dto.ExperimentId,
            ExperimentStageId = dto.ExperimentStageId,
            BatchId = dto.BatchId,
            MeasurementDefinitionId = dto.MeasurementDefinitionId,
            MeasuredBy = measuredBy,
            Value = dto.Value,
            TextValue = dto.TextValue,
            ExtraData = dto.ExtraData != null ? JsonSerializer.Serialize(dto.ExtraData) : null,
            MeasuredAt = dto.MeasuredAt ?? DateTime.UtcNow
        };

        await _recordRepository.CreateAsync(record);
        return await MapToResponseDto(record);
    }

    public async System.Threading.Tasks.Task<MeasurementRecordResponseDto> UpdateAsync(Guid id, UpdateMeasurementRecordDto dto, Guid userId)
    {
        var existing = await _recordRepository.GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException("Bản ghi không tìm thấy.");

        var validationError = ValidateUpdate(dto);
        if (validationError != null) throw new ArgumentException(validationError);

        if (dto.Value.HasValue) existing.Value = dto.Value;
        if (dto.TextValue != null) existing.TextValue = dto.TextValue;
        if (dto.ExtraData != null) existing.ExtraData = JsonSerializer.Serialize(dto.ExtraData);
        if (dto.MeasuredAt.HasValue) existing.MeasuredAt = dto.MeasuredAt.Value;

        await _recordRepository.UpdateAsync(existing);
        return await MapToResponseDto(existing);
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(Guid id)
    {
        var record = await _recordRepository.GetByIdAsync(id);
        if (record == null) throw new KeyNotFoundException("Bản ghi không tìm thấy.");

        await _recordRepository.SoftDeleteAsync(id);
        return true;
    }

    public async System.Threading.Tasks.Task<MeasurementRecordResponseDto> GetByIdAsync(Guid id)
    {
        var record = await _recordRepository.GetByIdAsync(id);
        if (record == null) throw new KeyNotFoundException("Bản ghi không tìm thấy.");
        return await MapToResponseDto(record);
    }

    public async System.Threading.Tasks.Task<List<MeasurementRecordResponseDto>> GetByBatchIdAsync(Guid batchId)
    {
        var records = await _recordRepository.GetByBatchIdAsync(batchId);
        return MapToResponseDtoList(records);
    }

    public async System.Threading.Tasks.Task<List<MeasurementRecordResponseDto>> GetByExperimentIdAsync(Guid experimentId)
    {
        var records = await _recordRepository.GetByExperimentIdAsync(experimentId);
        return MapToResponseDtoList(records);
    }

    public async System.Threading.Tasks.Task<List<MeasurementRecordResponseDto>> GetByStageIdAsync(Guid stageId)
    {
        var records = await _recordRepository.GetByStageIdAsync(stageId);
        return MapToResponseDtoList(records);
    }

    private static string? ValidateCreate(CreateMeasurementRecordDto dto)
    {
        if (dto.ExperimentId == Guid.Empty) return "ExperimentId là bắt buộc.";
        if (dto.BatchId == Guid.Empty) return "BatchId là bắt buộc.";

        if (dto.Value.HasValue && dto.TextValue != null)
            return "Chỉ được cung cấp một trong hai: Value hoặc TextValue, không được cả hai.";
        if (!dto.Value.HasValue && string.IsNullOrWhiteSpace(dto.TextValue))
            return "Phải cung cấp Value hoặc TextValue.";

        if (dto.Value.HasValue && dto.Value < 0)
            return "Value phải lớn hơn hoặc bằng 0.";

        var maxAllowed = DateTime.UtcNow.AddMinutes(ClockSkewMinutes);
        if (dto.MeasuredAt.HasValue && dto.MeasuredAt.Value > maxAllowed)
            return $"MeasuredAt không được lớn hơn thời gian hiện tại (tolerance: {ClockSkewMinutes} phút).";

        return null;
    }

    private static string? ValidateUpdate(UpdateMeasurementRecordDto dto)
    {
        if (dto.Value.HasValue && dto.TextValue != null)
            return "Chỉ được cung cấp một trong hai: Value hoặc TextValue, không được cả hai.";

        if (dto.Value.HasValue && dto.Value < 0)
            return "Value phải lớn hơn hoặc bằng 0.";

        var maxAllowed = DateTime.UtcNow.AddMinutes(ClockSkewMinutes);
        if (dto.MeasuredAt.HasValue && dto.MeasuredAt.Value > maxAllowed)
            return $"MeasuredAt không được lớn hơn thời gian hiện tại (tolerance: {ClockSkewMinutes} phút).";

        return null;
    }

    private List<MeasurementRecordResponseDto> MapToResponseDtoList(List<MeasurementRecord> records)
    {
        var results = new List<MeasurementRecordResponseDto>();
        foreach (var r in records)
            results.Add(MapToResponseDto(r).GetAwaiter().GetResult());
        return results;
    }

    private async System.Threading.Tasks.Task<MeasurementRecordResponseDto> MapToResponseDto(MeasurementRecord record)
    {
        object? parsedExtra = null;
        if (!string.IsNullOrEmpty(record.ExtraData))
        {
            try { parsedExtra = JsonSerializer.Deserialize<object>(record.ExtraData); }
            catch { parsedExtra = record.ExtraData; }
        }

        return new MeasurementRecordResponseDto
        {
            Id = record.Id,
            ExperimentId = record.ExperimentId,
            ExperimentTitle = record.Experiment?.Title,
            ExperimentStageId = record.ExperimentStageId,
            ExperimentStageName = record.ExperimentStage?.StageName,
            BatchId = record.BatchId,
            BatchCode = record.Batch?.BatchCode,
            MeasurementDefinitionId = record.MeasurementDefinitionId,
            MetricName = record.MeasurementDefinition?.MetricName,
            Unit = record.MeasurementDefinition?.Unit,
            TargetValue = record.MeasurementDefinition?.TargetValue,
            Value = record.Value,
            TextValue = record.TextValue,
            ExtraData = parsedExtra,
            MeasuredBy = record.MeasuredBy,
            MeasuredByName = record.MeasuredByNavigation?.FullName,
            MeasuredAt = record.MeasuredAt
        };
    }
}
