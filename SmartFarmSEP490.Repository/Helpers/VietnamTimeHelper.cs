namespace SmartFarmSEP490.Repository.Helpers;

/// <summary>
/// Helpers convert giữa giờ Việt Nam (ICT = UTC+7) và UTC. VN không có DST nên offset cố định +7.
/// Repository project không reference Service, nên duplicate logic VietnamTime.
/// </summary>
internal static class VietnamTimeHelper
{
    public const int VietnamUtcOffsetHours = 7;
    public const int DailyDeadlineHour = 17; // 17:00 ICT = 10:00 UTC

    /// <summary>UTC hiện tại.</summary>
    public static DateTime NowUtc() => DateTime.UtcNow;

    /// <summary>Convert 1 DateTime (Unspecified coi như giờ VN) sang UTC.</summary>
    public static DateTime ToUtcFromVietnam(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.AddHours(-VietnamUtcOffsetHours), DateTimeKind.Utc)
        };
    }

/// <summary>
/// Trả về (startUtc, endUtc) của ngày hiện tại theo ICT (00:00 ICT → 00:00 ICT ngày mai).
/// Mặc định dùng cho các filter "hôm nay".
/// </summary>
public static (DateTime startUtc, DateTime endUtc) GetVietnamDayWindowUtc(DateTime? asUtc = null)
{
    var nowUtc = asUtc ?? DateTime.UtcNow;
    var nowVietnam = nowUtc.AddHours(VietnamUtcOffsetHours);
    var startVietnam = nowVietnam.Date;
    var endVietnam = startVietnam.AddDays(1);
    var startUtc = DateTime.SpecifyKind(startVietnam.AddHours(-VietnamUtcOffsetHours), DateTimeKind.Utc);
    var endUtc = DateTime.SpecifyKind(endVietnam.AddHours(-VietnamUtcOffsetHours), DateTimeKind.Utc);
    return (startUtc, endUtc);
}

/// <summary>
/// Trả về (startUtc, endUtc) của "giờ làm việc hôm nay" theo ICT (00:00 → 17:00 ICT = deadline).
/// Dùng cho filter "today" của task và cửa sổ reminder.
/// Nếu đã qua 17:00 ICT, trả về cửa sổ [00:00 → 17:00] của ngày hôm nay (không phải ngày mai).
/// </summary>
public static (DateTime startUtc, DateTime endUtc) GetVietnamWorkdayWindowUtc(DateTime? asUtc = null)
{
    var (startUtcDay, _) = GetVietnamDayWindowUtc(asUtc);
    var endVietnam = startUtcDay.AddHours(VietnamUtcOffsetHours).Date.AddHours(DailyDeadlineHour);
    var endUtc = DateTime.SpecifyKind(endVietnam.AddHours(-VietnamUtcOffsetHours), DateTimeKind.Utc);
    return (startUtcDay, endUtc);
}

/// <summary>Trả về deadline mặc định của Task trong ngày (UTC), tức 17:00 ICT = 10:00 UTC cùng ngày theo ICT.</summary>
public static DateTime GetVietnamDailyDeadlineUtc(DateOnly date)
{
    var dueVietnam = date.ToDateTime(new TimeOnly(DailyDeadlineHour, 0, 0));
    return DateTime.SpecifyKind(dueVietnam.AddHours(-VietnamUtcOffsetHours), DateTimeKind.Utc);
}
}