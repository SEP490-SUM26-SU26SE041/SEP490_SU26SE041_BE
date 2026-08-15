namespace SmartFarmSEP490.Model.Helpers;

/// <summary>
/// Helpers convert giữa giờ Việt Nam (ICT = UTC+7) và UTC.
/// Server lưu mọi DateTime theo UTC; FE Việt Nam gửi giờ local → backend convert sang UTC trước khi lưu.
/// VN không có DST nên offset cố định +7.
/// Đặt trong Model vì là cross-cutting utility: cả Service và Repository đều cần dùng.
/// </summary>
public static class VietnamTime
{
    public const string VietnamTimeZoneId = "SE Asia Standard Time"; // Windows TZ id, UTC+7
    public const int VietnamUtcOffsetHours = 7;
    /// <summary>Deadline mặc định trong ngày cho Task = 17:00 ICT = 10:00 UTC.</summary>
    public const int DailyDeadlineHour = 17;
    private static readonly TimeZoneInfo VietnamTz = TimeZoneInfo.FindSystemTimeZoneById(VietnamTimeZoneId);

    /// <summary>Lấy thời điểm hiện tại theo UTC.</summary>
    public static DateTime NowUtc() => DateTime.UtcNow;

    /// <summary>Convert từ UTC sang giờ Việt Nam. Kết quả Kind=Unspecified để tránh bị hiểu nhầm là Local server TZ.</summary>
    public static DateTime ToVietnam(DateTime utc)
    {
        var ensuredUtc = EnsureUtc(utc);
        var vietnam = TimeZoneInfo.ConvertTimeFromUtc(ensuredUtc, VietnamTz);
        return DateTime.SpecifyKind(vietnam, DateTimeKind.Unspecified);
    }

    /// <summary>Convert từ giờ Việt Nam sang UTC để lưu DB.</summary>
    public static DateTime ToUtcFromVietnam(DateTime vietnamLocal)
    {
        // Nếu FE gửi DateTime.Kind=Unspecified (không có offset), ta giả định đó là giờ VN
        if (vietnamLocal.Kind == DateTimeKind.Unspecified)
        {
            return TimeZoneInfo.ConvertTimeToUtc(vietnamLocal, VietnamTz);
        }

        // Nếu đã là UTC thì trả về
        if (vietnamLocal.Kind == DateTimeKind.Utc) return vietnamLocal;

        // Kind=Local: ConvertTimeFromUtc ở hàm ToVietnam() đã set Kind=Local nhưng giá trị vẫn là giờ VN.
        // Ta coi đây vẫn là giờ VN, chỉ cần trừ 7h để ra UTC — bất kể server TZ là gì.
        return DateTime.SpecifyKind(vietnamLocal.AddHours(-VietnamUtcOffsetHours), DateTimeKind.Utc);
    }

    /// <summary>Chuẩn hóa 1 DateTime về UTC. Nếu Kind=Unspecified thì coi như đã là UTC.</summary>
    public static DateTime EnsureUtc(DateTime dt)
    {
        return dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
    }

    /// <summary>Trả về DateTimeOffset ISO-8601 với offset +07:00 (cho FE hiển thị).</summary>
    public static DateTimeOffset ToVietnamOffset(DateTime utc)
    {
        var ensuredUtc = EnsureUtc(utc);
        var vietnam = TimeZoneInfo.ConvertTimeFromUtc(ensuredUtc, VietnamTz);
        var offset = VietnamTz.GetUtcOffset(vietnam);
        return new DateTimeOffset(vietnam, offset);
    }

    /// <summary>Trả về DateTimeOffset ISO-8601 UTC (cho response mặc định — tương thích ISO).</summary>
    public static DateTimeOffset ToUtcOffset(DateTime utc)
    {
        return new DateTimeOffset(EnsureUtc(utc), TimeSpan.Zero);
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
