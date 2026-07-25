namespace SmartFarmSEP490.Service.Helpers;

/// <summary>
/// Helpers convert giữa giờ Việt Nam (ICT = UTC+7) và UTC.
/// Server lưu mọi DateTime theo UTC; FE Việt Nam gửi giờ local → backend convert sang UTC trước khi lưu.
/// </summary>
public static class VietnamTime
{
    public const string VietnamTimeZoneId = "SE Asia Standard Time"; // Windows TZ id, UTC+7
    private static readonly TimeZoneInfo VietnamTz = TimeZoneInfo.FindSystemTimeZoneById(VietnamTimeZoneId);

    /// <summary>Lấy thời điểm hiện tại theo UTC.</summary>
    public static DateTime NowUtc() => DateTime.UtcNow;

    /// <summary>Convert từ UTC sang giờ Việt Nam.</summary>
    public static DateTime ToVietnam(DateTime utc)
    {
        var ensuredUtc = EnsureUtc(utc);
        return TimeZoneInfo.ConvertTimeFromUtc(ensuredUtc, VietnamTz);
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

        // Nếu là Local (server timezone), convert trước rồi mới tính
        var asUtc = vietnamLocal.Kind == DateTimeKind.Local
            ? TimeZoneInfo.ConvertTimeToUtc(vietnamLocal)
            : vietnamLocal;

        var vnFromUtc = TimeZoneInfo.ConvertTimeFromUtc(asUtc, VietnamTz);
        return TimeZoneInfo.ConvertTimeToUtc(vnFromUtc, VietnamTz);
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
}
