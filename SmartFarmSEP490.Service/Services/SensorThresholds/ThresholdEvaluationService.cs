using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Model.Helpers;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Alerts;
using SmartFarmSEP490.Service.Interfaces.Notifications;
using SmartFarmSEP490.Service.Interfaces.SensorThresholds;
using SmartFarmSEP490.Service.WebSockets;

namespace SmartFarmSEP490.Service.Services.SensorThresholds;

/// <summary>
/// Implementation của IThresholdEvaluationService.
/// Được gọi từ MqttMessageHandler SAU KHI đã lưu SensorDatum thành công.
/// </summary>
/// <remarks>
/// <para>Flow:</para>
/// <list type="number">
/// <item>Tìm rule áp dụng (gọi ISensorThresholdRuleService.FindApplicableRuleAsync).</item>
/// <item>So sánh value với MinValue/MaxValue của rule.</item>
/// <item>Nếu vi phạm → kiểm tra spam (alert cùng loại trong 1 phút).</item>
/// <item>Nếu không spam → tạo Alert với Severity từ rule.</item>
/// <item><b>MỚI:</b> Push realtime tới Researcher sở hữu experiment qua Notification + WebSocket.</item>
/// </list>
/// </remarks>
public class ThresholdEvaluationService : IThresholdEvaluationService
{
    private readonly ISensorThresholdRuleService _ruleService;
    private readonly IAlertRepository _alertRepository;
    private readonly INotificationService _notificationService;
    private readonly IWebSocketConnectionManager _wsManager;
    private readonly SmartFarmDbContext _context;
    private readonly ILogger<ThresholdEvaluationService> _logger;

    // Ngưỡng chống spam: không tạo Alert mới nếu đã có Alert unresolved
    // cho cùng (sensor, rule) trong vòng N phút.
    private const int SPAM_PROTECTION_MINUTES = 1;

    // Tên event WebSocket mà frontend subscribe để cập nhật dashboard alert realtime.
    private const string WS_EVENT_ALERT = "ReceiveAlert";

    public ThresholdEvaluationService(
        ISensorThresholdRuleService ruleService,
        IAlertRepository alertRepository,
        INotificationService notificationService,
        IWebSocketConnectionManager wsManager,
        SmartFarmDbContext context,
        ILogger<ThresholdEvaluationService> logger)
    {
        _ruleService = ruleService;
        _alertRepository = alertRepository;
        _notificationService = notificationService;
        _wsManager = wsManager;
        _context = context;
        _logger = logger;
    }

    public async Task<ThresholdCheckResult> EvaluateAsync(Sensor sensor, SensorDatum reading)
    {
        var result = new ThresholdCheckResult();

        try
        {
            // Bước 1: Tìm rule áp dụng
            var rule = await _ruleService.FindApplicableRuleAsync(
                sensor.SensorType,
                reading.BatchId,
                reading.ExperimentId);

            if (rule == null)
            {
                // Không có rule nào → không có gì để check
                _logger.LogDebug(
                    "[Threshold] No rule for Sensor={SensorCode} Type={SensorType} BatchId={BatchId}",
                    sensor.SensorCode, sensor.SensorType, reading.BatchId);
                return result;
            }

            result.ViolatedRule = rule;

            // Bước 2: So sánh value với Min/Max
            string? violationReason = null;
            if (rule.MinValue.HasValue && reading.Value < rule.MinValue.Value)
            {
                violationReason = $"BelowMinValue (value={reading.Value}, min={rule.MinValue})";
            }
            else if (rule.MaxValue.HasValue && reading.Value > rule.MaxValue.Value)
            {
                violationReason = $"AboveMaxValue (value={reading.Value}, max={rule.MaxValue})";
            }

            if (violationReason == null)
            {
                // Không vi phạm
                return result;
            }

            result.IsViolated = true;
            result.ViolationReason = violationReason;

            _logger.LogWarning(
                "[Threshold] ⚠️ Sensor {SensorCode} VIOLATED rule {RuleId}: {Reason}",
                sensor.SensorCode, rule.Id, violationReason);

            // Bước 3: Kiểm tra spam protection
            var recentAlert = await _context.Alerts
                .Where(a => a.SensorId == sensor.Id
                         && a.IsResolved == false
                         && a.Title != null
                         && a.Title.Contains(rule.Id.ToString().Substring(0, 8)))
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (recentAlert != null &&
                (DateTime.UtcNow - recentAlert.CreatedAt).TotalMinutes < SPAM_PROTECTION_MINUTES)
            {
                _logger.LogInformation(
                    "[Threshold] ⏸️ Skip alert (spam protection): recent alert created {Seconds:F0}s ago",
                    (DateTime.UtcNow - recentAlert.CreatedAt).TotalSeconds);
                return result;
            }

            // Bước 4: Tạo Alert với Severity từ rule
            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                ExperimentId = reading.ExperimentId,
                BatchId = reading.BatchId,
                SensorId = sensor.Id,
                Title = BuildAlertTitle(sensor, rule, reading, violationReason),
                Message = BuildAlertMessage(sensor, rule, reading, violationReason),
                Severity = rule.Severity,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            };

            await _alertRepository.AddAsync(alert);
            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "[Threshold] 🚨 Alert created: Id={AlertId}, Severity={Severity}, Sensor={SensorCode}, Rule={RuleId}",
                alert.Id, alert.Severity, sensor.SensorCode, rule.Id);

            // Bước 5: Push realtime tới Researcher sở hữu experiment.
            // Mỗi notification service tự gọi wsManager.SendToUserAsync bên trong,
            // nên frontend nhận cả 2 event: "ReceiveNotification" (chuông) + "ReceiveAlert" (dashboard).
            try
            {
                await PushRealtimeNotificationAsync(alert, sensor);
            }
            catch (Exception ex)
            {
                // Realtime failure KHÔNG được làm fail cả flow threshold (alert đã lưu DB rồi).
                _logger.LogWarning(ex,
                    "[Threshold] Failed to push realtime notification for AlertId={AlertId}",
                    alert.Id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[Threshold] Error evaluating threshold for Sensor={SensorCode}, SensorDataId={SensorDataId}",
                sensor.SensorCode, reading.Id);
            return result;
        }
    }

    private static string BuildAlertTitle(Sensor sensor, SensorThresholdRuleDto rule, SensorDatum reading, string reason)
    {
        var sensorLabel = sensor.SensorCode;
        var typeLabel = sensor.SensorType.ToString();
        var shortRuleId = rule.Id.ToString().Substring(0, 8);

        return reason.StartsWith("Below")
            ? $"[{typeLabel}] {sensorLabel} dưới ngưỡng ({reading.Value} < {rule.MinValue})"
            : $"[{typeLabel}] {sensorLabel} vượt ngưỡng ({reading.Value} > {rule.MaxValue})";
    }

    private static string BuildAlertMessage(Sensor sensor, SensorThresholdRuleDto rule, SensorDatum reading, string reason)
    {
        var customMsg = !string.IsNullOrWhiteSpace(rule.Message)
            ? rule.Message + "\n\n"
            : "";

        var ruleInfo = $"Rule: [{rule.MinValue?.ToString() ?? "-∞"} → {rule.MaxValue?.ToString() ?? "+∞"}]";
        var sensorInfo = $"Sensor: {sensor.SensorCode} ({sensor.SensorType})";
        var valueInfo = $"Giá trị đo: {reading.Value} lúc {reading.RecordedAt:yyyy-MM-dd HH:mm:ss} UTC";

        return $"{customMsg}{sensorInfo}\n{valueInfo}\n{ruleInfo}\nLý do: {reason}";
    }

    /// <summary>
    /// Push realtime alert tới Researcher sở hữu experiment.
    /// Tái sử dụng INotificationService (giống Task quá hạn) + thêm 1 event riêng
    /// "ReceiveAlert" để dashboard cập nhật nhanh mà không cần refetch.
    /// </summary>
    /// <remarks>
    /// <para>Recipient = Researcher sở hữu Experiment (giống logic Task).</para>
    /// <para>Nếu không tìm được Researcher (Experiment null) → fallback log warning, bỏ qua.</para>
    /// </remarks>
    private async Task PushRealtimeNotificationAsync(Alert alert, Sensor sensor)
    {
        // 1. Lấy Experiment để biết ResearcherId (giống OverdueTaskService dùng task.CreatedBy)
        if (!alert.ExperimentId.HasValue)
        {
            _logger.LogWarning(
                "[Threshold] AlertId={AlertId} has no ExperimentId, skip realtime push.",
                alert.Id);
            return;
        }

        var experiment = await _context.Experiments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == alert.ExperimentId.Value);

        if (experiment == null || experiment.ResearcherId == Guid.Empty)
        {
            _logger.LogWarning(
                "[Threshold] Cannot resolve ResearcherId for ExperimentId={ExperimentId}. Skip realtime.",
                alert.ExperimentId);
            return;
        }

        var recipientId = experiment.ResearcherId;

        // 2. Tạo Notification DB (giống TaskOverdue) → notification service tự push WS event "ReceiveNotification"
        //    Severity enum → string để khớp CreateNotificationDto.Priority.
        var notificationDto = await _notificationService.PushNotificationAsync(new CreateNotificationDto
        {
            RecipientId = recipientId,
            NotificationType = "SensorThresholdViolation",
            Title = alert.Title,
            Message = alert.Message,
            Priority = alert.Severity.ToString(),  // "Low" / "Medium" / "High" / "Critical"
            ReferenceTable = "Alert",
            ReferenceId = alert.Id
        });

        _logger.LogInformation(
            "[Threshold] 📣 Notification pushed to ResearcherId={RecipientId}, NotificationId={NotificationId}",
            recipientId, notificationDto.Id);

        // 3. Push thêm 1 event riêng "ReceiveAlert" cho Dashboard cập nhật nhanh
        //    (tách khỏi notification vì dashboard cần full Alert object chứ không phải notification).
        //    Lưu ý: không await fail (đã có try/catch ở caller).
        var alertEvent = new AlertEventDto
        {
            AlertId = alert.Id,
            ExperimentId = alert.ExperimentId,
            BatchId = alert.BatchId,
            SensorId = alert.SensorId,
            SensorCode = sensor.SensorCode,
            SensorType = sensor.SensorType,
            Title = alert.Title,
            Message = alert.Message,
            Severity = alert.Severity,
            CreatedAt = alert.CreatedAt,
            CreatedAtVietnam = VietnamTime.ToVietnamOffset(alert.CreatedAt)
        };

        var sent = await _wsManager.SendToUserAsync(recipientId, WS_EVENT_ALERT, alertEvent);
        if (sent == 0)
        {
            _logger.LogDebug(
                "[Threshold] ResearcherId={RecipientId} is offline (no WS connection). Alert saved to DB; will show on next login.",
                recipientId);
        }
        else
        {
            _logger.LogInformation(
                "[Threshold] ⚡ WS 'ReceiveAlert' sent to {Count} connection(s) of ResearcherId={RecipientId}",
                sent, recipientId);
        }
    }
}