using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

/// <summary>
/// Ngưỡng cảnh báo cho cảm biến, cấu hình theo Batch hoặc Experiment.
/// </summary>
/// <remarks>
/// <para>Quy tắc áp dụng:</para>
/// <list type="bullet">
/// <item>
/// <term>SensorType = NULL</term>
/// <description>Rule áp dụng cho MỌI loại cảm biến trong Batch/Experiment.</description>
/// </item>
/// <item>
/// <term>SensorType != NULL</term>
/// <description>Rule chỉ áp dụng cho đúng loại cảm biến đó.</description>
/// </item>
/// </list>
/// <para>Ví dụ:</para>
/// <list type="bullet">
/// <item>Rule (BatchId=B1, SensorType=Temperature, Min=20, Max=30) → chỉ check sensor nhiệt độ.</item>
/// <item>Rule (BatchId=B1, SensorType=NULL, Min=20, Max=30) → check tất cả sensor (nhiệt độ, độ ẩm...).</item>
/// </list>
/// </remarks>
public partial class SensorThresholdRule
{
    public Guid Id { get; set; }

    /// <summary>
    /// ID của Experiment áp dụng rule. Bắt buộc.
    /// </summary>
    public Guid ExperimentId { get; set; }

    /// <summary>
    /// ID của Batch áp dụng rule. NULL = áp dụng cho toàn Experiment.
    /// </summary>
    public Guid? BatchId { get; set; }

    /// <summary>
    /// Loại cảm biến áp dụng rule. NULL = áp dụng cho mọi loại cảm biến.
    /// </summary>
    public SensorType? SensorType { get; set; }

    /// <summary>
    /// Giá trị tối thiểu. NULL = không kiểm tra cận dưới.
    /// </summary>
    public decimal? MinValue { get; set; }

    /// <summary>
    /// Giá trị tối đa. NULL = không kiểm tra cận trên.
    /// </summary>
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// Mức độ nghiêm trọng của cảnh báo. Mặc định = Medium.
    /// </summary>
    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;

    /// <summary>
    /// Nội dung thông báo tùy chỉnh.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Có đang kích hoạt rule này không. Mặc định = true.
    /// </summary>
    public bool IsActive { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Experiment Experiment { get; set; } = null!;
}
