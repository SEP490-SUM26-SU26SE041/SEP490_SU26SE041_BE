using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model.DTOs;

/// <summary>
/// DTO trả về thông tin một SensorThresholdRule.
/// </summary>
public class SensorThresholdRuleDto
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public Guid? BatchId { get; set; }

    /// <summary>
    /// Loại cảm biến áp dụng. NULL = áp dụng cho mọi loại.
    /// </summary>
    public SensorType? SensorType { get; set; }

    public decimal? MinValue { get; set; }

    public decimal? MaxValue { get; set; }

    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;

    public string? Message { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// Thông tin bổ sung (optional - load kèm theo).
    /// </summary>
    public string? BatchCode { get; set; }
    public string? ExperimentCode { get; set; }
}

/// <summary>
/// DTO để tạo mới SensorThresholdRule.
/// </summary>
/// <remarks>
/// <para>Validate:</para>
/// <list type="bullet">
/// <item><b>BatchId bắt buộc</b> — suy ra ExperimentId từ Batch (mỗi batch thuộc đúng 1 experiment).</item>
/// <item>Nếu có MinValue và MaxValue thì MinValue phải nhỏ hơn MaxValue.</item>
/// <item>SensorType có thể NULL → rule áp dụng cho mọi loại cảm biến trong batch.</item>
/// </list>
/// <para><b>Lưu ý:</b> DTO này chỉ tạo rule gắn với 1 batch cụ thể. Để tạo rule áp dụng cho toàn
/// experiment (BatchId=null), dùng endpoint <c>POST /api/threshold-rules/experiment-wide</c>.</para>
/// </remarks>
public class CreateSensorThresholdRuleDto
{
    /// <summary>
    /// ID của Batch áp dụng rule. <b>Bắt buộc</b>.
    /// </summary>
    /// <remarks>
    /// ExperimentId được tự động suy ra từ <c>Batch.ExperimentId</c>, tránh truyền thừa.
    /// </remarks>
    [Required(ErrorMessage = "BatchId là bắt buộc. Mỗi rule phải gắn với 1 batch cụ thể.")]
    public Guid BatchId { get; set; }

    /// <summary>
    /// Loại cảm biến. NULL = áp dụng cho mọi loại.
    /// </summary>
    public SensorType? SensorType { get; set; }

    public decimal? MinValue { get; set; }

    public decimal? MaxValue { get; set; }

    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;

    [StringLength(500, ErrorMessage = "Message không được vượt quá 500 ký tự.")]
    public string? Message { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO tạo rule áp dụng cho TOÀN experiment (không gắn batch cụ thể).
/// Dùng cho các rule generic chung (ví dụ: "nếu sensor trả về 0 thì cảnh báo offline",
/// "pH > 8 nguy hiểm cho mọi batch", "nhiệt độ > 40°C → Critical").
/// </summary>
/// <remarks>
/// <para><b>Khác với <see cref="CreateSensorThresholdRuleDto"/>:</b></para>
/// <list type="bullet">
/// <item><c>CreateSensorThresholdRuleDto</c> → rule cho <b>1 batch cụ thể</b> (bắt buộc BatchId).</item>
/// <item><c>CreateExperimentWideRuleDto</c> → rule cho <b>mọi batch</b> trong experiment (BatchId=null).</item>
/// </list>
/// <para>Rule experiment-wide giúp:</para>
/// <list type="bullet">
/// <item>Không phải tạo N rule cho N batch.</item>
/// <item>Batch mới thêm vào sau → tự động được áp dụng rule.</item>
/// </list>
/// <para>Endpoint: <c>POST /api/threshold-rules/experiment-wide</c></para>
/// </remarks>
/// <example>
/// <code>
/// POST /api/threshold-rules/experiment-wide
/// {
///   "experimentId": "exp-guid",
///   "sensorType": null,           // null = mọi loại sensor
///   "minValue": null,
///   "maxValue": 40,
///   "severity": "Critical",
///   "message": "Sensor trả 0 = offline, hoặc giá trị bất thường"
/// }
/// </code>
/// </example>
public class CreateExperimentWideRuleDto
{
    [Required]
    public Guid ExperimentId { get; set; }

    public SensorType? SensorType { get; set; }

    public decimal? MinValue { get; set; }

    public decimal? MaxValue { get; set; }

    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;

    [StringLength(500)]
    public string? Message { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO để cập nhật SensorThresholdRule (tất cả field optional).
/// </summary>
public class UpdateSensorThresholdRuleDto
{
    public SensorType? SensorType { get; set; }

    public decimal? MinValue { get; set; }

    public decimal? MaxValue { get; set; }

    public AlertSeverity? Severity { get; set; }

    [StringLength(500)]
    public string? Message { get; set; }

    public bool? IsActive { get; set; }
}

/// <summary>
/// Kết quả check threshold cho 1 sensor reading.
/// </summary>
public class ThresholdCheckResult
{
    /// <summary>
    /// Có vi phạm ngưỡng hay không.
    /// </summary>
    public bool IsViolated { get; set; }

    /// <summary>
    /// Rule nào bị vi phạm (nếu có).
    /// </summary>
    public SensorThresholdRuleDto? ViolatedRule { get; set; }

    /// <summary>
    /// Lý do vi phạm: "BelowMinValue" / "AboveMaxValue".
    /// </summary>
    public string? ViolationReason { get; set; }
}

/// <summary>
/// Filter query cho danh sách rules.
/// </summary>
public class SensorThresholdRuleFilter
{
    public Guid? ExperimentId { get; set; }
    public Guid? BatchId { get; set; }
    public SensorType? SensorType { get; set; }
    public bool? IsActive { get; set; }
}
