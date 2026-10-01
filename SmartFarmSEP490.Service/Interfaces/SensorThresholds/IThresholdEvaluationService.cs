using System;
using System.Threading.Tasks;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Service.Interfaces.SensorThresholds;

/// <summary>
/// Service đánh giá threshold realtime - được gọi mỗi khi có sensor reading mới.
/// Có nhiệm vụ:
///   1. Tìm rule áp dụng cho sensor.
///   2. So sánh value với Min/Max.
///   3. Nếu vi phạm → tạo Alert với Severity tương ứng.
/// </summary>
public interface IThresholdEvaluationService
{
    /// <summary>
    /// Đánh giá 1 sensor reading vừa được ghi nhận.
    /// Nếu vi phạm ngưỡng → tự động tạo Alert (idempotent trong 1 phút để tránh spam).
    /// </summary>
    /// <param name="sensor">Entity Sensor (chứa SensorType).</param>
    /// <param name="reading">SensorDatum vừa được lưu.</param>
    /// <returns>Kết quả check - có vi phạm không, rule nào, alert đã tạo (nếu có).</returns>
    Task<ThresholdCheckResult> EvaluateAsync(Sensor sensor, SensorDatum reading);
}