using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Service.Interfaces.SensorThresholds;

namespace SmartFarmSEP490.Service.Services.SensorThresholds;

/// <summary>
/// Implementation của ISensorThresholdRuleService.
/// Thao tác trực tiếp trên DbContext (rule đơn giản, chưa cần tách Repository).
/// </summary>
public class SensorThresholdRuleService : ISensorThresholdRuleService
{
    private readonly SmartFarmDbContext _context;
    private readonly ILogger<SensorThresholdRuleService> _logger;

    public SensorThresholdRuleService(
        SmartFarmDbContext context,
        ILogger<SensorThresholdRuleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============== READ ==============

    public async Task<List<SensorThresholdRuleDto>> GetAllAsync(SensorThresholdRuleFilter? filter = null)
    {
        var query = _context.SensorThresholdRules
            .Include(r => r.Batch)
            .Include(r => r.Experiment)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.ExperimentId.HasValue)
                query = query.Where(r => r.ExperimentId == filter.ExperimentId.Value);
            if (filter.BatchId.HasValue)
                query = query.Where(r => r.BatchId == filter.BatchId.Value);
            if (filter.SensorType.HasValue)
                query = query.Where(r => r.SensorType == filter.SensorType.Value);
            if (filter.IsActive.HasValue)
                query = query.Where(r => r.IsActive == filter.IsActive.Value);
        }

        var rules = await query
            .OrderByDescending(r => r.IsActive)
            .ThenByDescending(r => r.Severity)
            .ToListAsync();

        return rules.Select(MapToDto).ToList();
    }

    public async Task<SensorThresholdRuleDto?> GetByIdAsync(Guid id)
    {
        var rule = await _context.SensorThresholdRules
            .Include(r => r.Batch)
            .Include(r => r.Experiment)
            .FirstOrDefaultAsync(r => r.Id == id);
        return rule == null ? null : MapToDto(rule);
    }

    public async Task<SensorThresholdRuleDto?> FindApplicableRuleAsync(
        SensorType sensorType,
        Guid? batchId,
        Guid? experimentId)
    {
        // Nếu không có experimentId thì không có rule nào áp dụng
        if (!experimentId.HasValue) return null;

        // ✅ Load 1 lần các rule active áp dụng được cho experiment
        var candidates = await _context.SensorThresholdRules
            .Where(r => r.IsActive && r.ExperimentId == experimentId.Value)
            .ToListAsync();

        // Lọc rule áp dụng được cho sensor:
        //  - SensorType = sensorType (rule riêng), HOẶC
        //  - SensorType = NULL (rule generic áp dụng cho mọi loại)
        // Lọc rule áp dụng được cho batch:
        //  - BatchId = batchId (rule riêng batch), HOẶC
        //  - BatchId = NULL (rule generic cho cả experiment)
        var applicable = candidates.Where(r =>
            (r.SensorType == null || r.SensorType == sensorType) &&
            (r.BatchId == null || r.BatchId == batchId)
        ).ToList();

        if (!applicable.Any()) return null;

        // Ưu tiên:
        //   1. Rule có SensorType cụ thể (HasValue = true)
        //   2. Rule có BatchId cụ thể
        //   3. Trong cùng mức: severity cao hơn trước
        var best = applicable
            .OrderByDescending(r => r.SensorType.HasValue)
            .ThenByDescending(r => r.BatchId.HasValue)
            .ThenByDescending(r => (int)r.Severity)
            .FirstOrDefault();

        return best == null ? null : MapToDto(best);
    }

    // ============== CREATE ==============

    public async Task<SensorThresholdRuleDto> CreateAsync(CreateSensorThresholdRuleDto dto)
    {
        // Validate Batch tồn tại — BatchId là bắt buộc trong DTO mới.
        // ExperimentId được tự suy ra từ Batch (mỗi batch thuộc đúng 1 experiment).
        var batch = await _context.Batches.FindAsync(dto.BatchId);
        if (batch == null)
            throw new KeyNotFoundException($"BatchId '{dto.BatchId}' không tồn tại.");

        var experimentId = batch.ExperimentId;

        // (Optional) Validate Experiment tồn tại — phòng trường hợp batch trỏ vào experiment rỗng.
        var experiment = await _context.Experiments.FindAsync(experimentId);
        if (experiment == null)
            throw new KeyNotFoundException($"ExperimentId '{experimentId}' (từ Batch '{batch.BatchCode}') không tồn tại.");

        // Validate MinValue < MaxValue nếu cả 2 có
        if (dto.MinValue.HasValue && dto.MaxValue.HasValue && dto.MinValue > dto.MaxValue)
            throw new InvalidOperationException(
                $"MinValue ({dto.MinValue}) không được lớn hơn MaxValue ({dto.MaxValue}).");

        var rule = new SensorThresholdRule
        {
            Id = Guid.NewGuid(),
            ExperimentId = experimentId,  // Suy ra từ Batch
            BatchId = dto.BatchId,
            SensorType = dto.SensorType,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            Severity = dto.Severity,
            Message = dto.Message,
            IsActive = dto.IsActive
        };

        _context.SensorThresholdRules.Add(rule);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "[ThresholdRule] Created: Id={Id}, Experiment={ExperimentId}, Batch={BatchId}, SensorType={SensorType}, [{Min}, {Max}], Severity={Severity}",
            rule.Id, rule.ExperimentId, rule.BatchId, rule.SensorType, rule.MinValue, rule.MaxValue, rule.Severity);

        return (await GetByIdAsync(rule.Id))!;
    }

    // ============== CREATE (Experiment-wide) ==============

    /// <summary>
    /// Tạo rule áp dụng cho TOÀN experiment (BatchId=null).
    /// Dùng cho rule generic chung (ví dụ: "sensor trả 0 → cảnh báo offline").
    /// </summary>
    public async Task<SensorThresholdRuleDto> CreateExperimentWideAsync(CreateExperimentWideRuleDto dto)
    {
        var experiment = await _context.Experiments.FindAsync(dto.ExperimentId);
        if (experiment == null)
            throw new KeyNotFoundException($"ExperimentId '{dto.ExperimentId}' không tồn tại.");

        if (dto.MinValue.HasValue && dto.MaxValue.HasValue && dto.MinValue > dto.MaxValue)
            throw new InvalidOperationException(
                $"MinValue ({dto.MinValue}) không được lớn hơn MaxValue ({dto.MaxValue}).");

        var rule = new SensorThresholdRule
        {
            Id = Guid.NewGuid(),
            ExperimentId = dto.ExperimentId,
            BatchId = null,  // ← Experiment-wide: không gắn batch
            SensorType = dto.SensorType,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            Severity = dto.Severity,
            Message = dto.Message,
            IsActive = dto.IsActive
        };

        _context.SensorThresholdRules.Add(rule);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "[ThresholdRule] Created (Experiment-wide): Id={Id}, Experiment={ExperimentId}, SensorType={SensorType}, [{Min}, {Max}], Severity={Severity}",
            rule.Id, rule.ExperimentId, rule.SensorType, rule.MinValue, rule.MaxValue, rule.Severity);

        return (await GetByIdAsync(rule.Id))!;
    }

    // ============== UPDATE ==============

    public async Task<SensorThresholdRuleDto> UpdateAsync(Guid id, UpdateSensorThresholdRuleDto dto)
    {
        var rule = await _context.SensorThresholdRules.FindAsync(id);
        if (rule == null)
            throw new KeyNotFoundException($"Không tìm thấy SensorThresholdRule với Id '{id}'.");

        // Update từng field nếu có giá trị
        if (dto.SensorType.HasValue || (dto.SensorType == null && dto.IsSensorTypeExplicitNull()))
            rule.SensorType = dto.SensorType;

        if (dto.MinValue.HasValue)
            rule.MinValue = dto.MinValue;

        if (dto.MaxValue.HasValue)
            rule.MaxValue = dto.MaxValue;

        if (dto.Severity.HasValue)
            rule.Severity = dto.Severity.Value;

        if (dto.Message != null)  // null = không thay đổi, "" = clear
            rule.Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message;

        if (dto.IsActive.HasValue)
            rule.IsActive = dto.IsActive.Value;

        // Validate MinValue < MaxValue sau khi update
        if (rule.MinValue.HasValue && rule.MaxValue.HasValue && rule.MinValue > rule.MaxValue)
            throw new InvalidOperationException(
                $"MinValue ({rule.MinValue}) không được lớn hơn MaxValue ({rule.MaxValue}).");

        await _context.SaveChangesAsync();

        _logger.LogInformation("[ThresholdRule] Updated: Id={Id}", rule.Id);

        return (await GetByIdAsync(rule.Id))!;
    }

    // ============== DELETE ==============

    public async Task<bool> DeleteAsync(Guid id)
    {
        var rule = await _context.SensorThresholdRules.FindAsync(id);
        if (rule == null) return false;

        _context.SensorThresholdRules.Remove(rule);
        await _context.SaveChangesAsync();

        _logger.LogInformation("[ThresholdRule] Deleted: Id={Id}", id);
        return true;
    }

    // ============== TOGGLE ==============

    public async Task<SensorThresholdRuleDto> ToggleActiveAsync(Guid id, bool isActive)
    {
        var rule = await _context.SensorThresholdRules.FindAsync(id);
        if (rule == null)
            throw new KeyNotFoundException($"Không tìm thấy SensorThresholdRule với Id '{id}'.");

        rule.IsActive = isActive;
        await _context.SaveChangesAsync();

        _logger.LogInformation("[ThresholdRule] Toggled: Id={Id} → IsActive={IsActive}", id, isActive);

        return (await GetByIdAsync(id))!;
    }

    // ============== PRIVATE ==============

    private static SensorThresholdRuleDto MapToDto(SensorThresholdRule r) => new()
    {
        Id = r.Id,
        ExperimentId = r.ExperimentId,
        BatchId = r.BatchId,
        SensorType = r.SensorType,
        MinValue = r.MinValue,
        MaxValue = r.MaxValue,
        Severity = r.Severity,
        Message = r.Message,
        IsActive = r.IsActive,
        BatchCode = r.Batch?.BatchCode,
        ExperimentCode = r.Experiment?.ExperimentCode
    };
}

/// <summary>
/// Extension để nhận biết client có chủ động set SensorType = null hay không.
/// </summary>
internal static class UpdateSensorThresholdRuleDtoExtensions
{
    public static bool IsSensorTypeExplicitNull(this UpdateSensorThresholdRuleDto dto)
    {
        // Trong JSON, "SensorType": null gửi qua .NET sẽ là null.
        // Cách đơn giản: nếu dto.SensorType == null thì chỉ update khi client muốn clear.
        // Tuy nhiên default của UpdateDto cũng là null → không phân biệt được.
        // → Tạm thời: nếu user muốn set SensorType = null thì dùng API riêng.
        return false;
    }
}