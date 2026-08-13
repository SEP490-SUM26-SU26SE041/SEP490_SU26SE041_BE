using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

    public async Task<MeasurementRecordResponseDto> CreateAsync(CreateMeasurementRecordDto dto, Guid measuredBy)
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

    public async Task<BulkCreateMeasurementResultDto> CreateBulkAsync(BulkCreateMeasurementRecordDto dto, Guid measuredBy)
    {
        if (dto.ExperimentId == Guid.Empty)
            throw new ArgumentException("ExperimentId là bắt buộc.");
        if (dto.BatchId == Guid.Empty)
            throw new ArgumentException("BatchId là bắt buộc.");
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Danh sách chỉ số (Items) là bắt buộc.");

        var maxAllowed = DateTime.UtcNow.AddMinutes(ClockSkewMinutes);
        var measuredAt = dto.MeasuredAt ?? DateTime.UtcNow;
        if (measuredAt > maxAllowed)
            throw new ArgumentException($"MeasuredAt không được lớn hơn thời gian hiện tại (tolerance: {ClockSkewMinutes} phút).");

        var definitionIds = dto.Items.Select(i => i.MeasurementDefinitionId).Distinct().ToList();
        var definitions = await _recordRepository.GetDefinitionsByIdsAsync(definitionIds);
        var defMap = definitions.ToDictionary(d => d.Id);

        var entities = new List<MeasurementRecord>();
        var warnings = new List<string>();
        var extraJson = dto.ExtraData != null ? JsonSerializer.Serialize(dto.ExtraData) : null;
        var seen = new HashSet<Guid>();
        int skipped = 0;

        foreach (var item in dto.Items)
        {
            if (item.MeasurementDefinitionId == Guid.Empty)
            {
                warnings.Add("Bỏ qua item: MeasurementDefinitionId rỗng.");
                skipped++;
                continue;
            }

            if (item.Value.HasValue && !string.IsNullOrWhiteSpace(item.TextValue))
            {
                warnings.Add($"Bỏ qua item {item.MeasurementDefinitionId}: vừa có Value vừa có TextValue.");
                skipped++;
                continue;
            }

            if (!item.Value.HasValue && string.IsNullOrWhiteSpace(item.TextValue))
            {
                warnings.Add($"Bỏ qua item {item.MeasurementDefinitionId}: thiếu Value hoặc TextValue.");
                skipped++;
                continue;
            }

            if (item.Value.HasValue && item.Value < 0)
            {
                warnings.Add($"Bỏ qua item {item.MeasurementDefinitionId}: Value không được âm.");
                skipped++;
                continue;
            }

            if (!defMap.TryGetValue(item.MeasurementDefinitionId, out var def))
            {
                warnings.Add($"Bỏ qua item: không tìm thấy chỉ số đo lường với Id={item.MeasurementDefinitionId}.");
                skipped++;
                continue;
            }

            if (def.ExperimentId != dto.ExperimentId)
            {
                warnings.Add($"Bỏ qua item {def.MetricName}: chỉ số không thuộc thực nghiệm {dto.ExperimentId}.");
                skipped++;
                continue;
            }

            if (!seen.Add(item.MeasurementDefinitionId))
            {
                warnings.Add($"Bỏ qua item trùng lặp: {def.MetricName}.");
                skipped++;
                continue;
            }

            entities.Add(new MeasurementRecord
            {
                Id = Guid.NewGuid(),
                ExperimentId = dto.ExperimentId,
                ExperimentStageId = dto.ExperimentStageId,
                BatchId = dto.BatchId,
                MeasurementDefinitionId = def.Id,
                MeasuredBy = measuredBy,
                Value = item.Value,
                TextValue = item.TextValue,
                ExtraData = extraJson,
                MeasuredAt = measuredAt
            });
        }

        var created = entities.Count == 0
            ? new List<MeasurementRecord>()
            : await _recordRepository.CreateBulkAsync(entities);

        var records = new List<MeasurementRecordResponseDto>();
        foreach (var e in created)
        {
            var dtoResult = MapToResponseDtoSimple(e, defMap);
            records.Add(dtoResult);
        }

        return new BulkCreateMeasurementResultDto
        {
            BatchId = dto.BatchId,
            ExperimentStageId = dto.ExperimentStageId,
            MeasuredAt = measuredAt,
            Created = created.Count,
            Skipped = skipped,
            Warnings = warnings,
            Records = records
        };
    }

    public async Task<MeasurementRecordResponseDto> UpdateAsync(Guid id, UpdateMeasurementRecordDto dto, Guid userId)
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

    public async Task<bool> DeleteAsync(Guid id)
    {
        var record = await _recordRepository.GetByIdAsync(id);
        if (record == null) throw new KeyNotFoundException("Bản ghi không tìm thấy.");

        await _recordRepository.SoftDeleteAsync(id);
        return true;
    }

    public async Task<MeasurementRecordResponseDto> GetByIdAsync(Guid id)
    {
        var record = await _recordRepository.GetByIdAsync(id);
        if (record == null) throw new KeyNotFoundException("Bản ghi không tìm thấy.");
        return await MapToResponseDto(record);
    }

    public async Task<List<MeasurementRecordResponseDto>> GetByBatchIdAsync(Guid batchId)
    {
        var records = await _recordRepository.GetByBatchIdAsync(batchId);
        return MapToResponseDtoList(records);
    }

    public async Task<List<MeasurementRecordResponseDto>> GetByExperimentIdAsync(Guid experimentId)
    {
        var records = await _recordRepository.GetByExperimentIdAsync(experimentId);
        return MapToResponseDtoList(records);
    }

    public async Task<List<MeasurementRecordResponseDto>> GetByStageIdAsync(Guid stageId)
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

    private static MeasurementRecordResponseDto MapToResponseDtoSimple(
        MeasurementRecord record,
        Dictionary<Guid, MeasurementDefinition> defMap)
    {
        object? parsedExtra = null;
        if (!string.IsNullOrEmpty(record.ExtraData))
        {
            try { parsedExtra = JsonSerializer.Deserialize<object>(record.ExtraData); }
            catch { parsedExtra = record.ExtraData; }
        }

        defMap.TryGetValue(record.MeasurementDefinitionId ?? Guid.Empty, out var def);

        return new MeasurementRecordResponseDto
        {
            Id = record.Id,
            ExperimentId = record.ExperimentId,
            ExperimentStageId = record.ExperimentStageId,
            BatchId = record.BatchId,
            MeasurementDefinitionId = record.MeasurementDefinitionId,
            MetricName = def?.MetricName,
            Unit = def?.Unit,
            TargetValue = def?.TargetValue,
            Value = record.Value,
            TextValue = record.TextValue,
            ExtraData = parsedExtra,
            MeasuredBy = record.MeasuredBy,
            MeasuredAt = record.MeasuredAt
        };
    }

    private async Task<MeasurementRecordResponseDto> MapToResponseDto(MeasurementRecord record)
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
