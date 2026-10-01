using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.SensorThresholds;

/// <summary>
/// Service quản lý SensorThresholdRule (CRUD + filter).
/// </summary>
public interface ISensorThresholdRuleService
{
    /// <summary>
    /// Lấy tất cả rules (có thể filter).
    /// </summary>
    Task<List<SensorThresholdRuleDto>> GetAllAsync(SensorThresholdRuleFilter? filter = null);

    /// <summary>
    /// Lấy rule theo Id.
    /// </summary>
    Task<SensorThresholdRuleDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Tạo rule mới (luôn gắn với 1 batch cụ thể; ExperimentId tự suy ra từ Batch).
    /// </summary>
    Task<SensorThresholdRuleDto> CreateAsync(CreateSensorThresholdRuleDto dto);

    /// <summary>
    /// Tạo rule áp dụng cho toàn experiment (BatchId=null).
    /// </summary>
    Task<SensorThresholdRuleDto> CreateExperimentWideAsync(CreateExperimentWideRuleDto dto);

    /// <summary>
    /// Cập nhật rule (partial update).
    /// </summary>
    Task<SensorThresholdRuleDto> UpdateAsync(Guid id, UpdateSensorThresholdRuleDto dto);

    /// <summary>
    /// Xóa rule.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Bật/tắt nhanh 1 rule.
    /// </summary>
    Task<SensorThresholdRuleDto> ToggleActiveAsync(Guid id, bool isActive);

    /// <summary>
    /// Tìm rule áp dụng cho 1 sensor reading (dùng cho evaluation realtime).
    /// </summary>
    /// <param name="sensorType">Loại cảm biến của sensor.</param>
    /// <param name="batchId">Batch của sensor (có thể null).</param>
    /// <param name="experimentId">Experiment của sensor (có thể null nếu sensor chưa gán).</param>
    /// <returns>Rule cụ thể nhất áp dụng được (ưu tiên SensorType khớp > generic; Batch > Experiment). Trả null nếu experimentId null.</returns>
    Task<SensorThresholdRuleDto?> FindApplicableRuleAsync(
        Model.Enums.SensorType sensorType,
        Guid? batchId,
        Guid? experimentId);
}