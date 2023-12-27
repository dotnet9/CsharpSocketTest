namespace SocketCore.Utils;

public static class TimestampHelper
{
    private static readonly DateTime UtcTime = new(1970, 1, 1, 0, 0, 0, 0);

    public static long ToUtcTimestamp(this DateTime dateTime)
    {
        var ts = dateTime - UtcTime;
        return (long)ts.TotalMilliseconds;
    }

    public static uint ToTodayTimestamp(this DateTime dateTime)
    {
        var time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
        var ts = dateTime - time;
        return (uint)ts.TotalMilliseconds;
    }

    public static long GetCurrentUtcTimestamp()
    {
        return ToUtcTimestamp(DateTime.Now);
    }

    public static uint GetCurrentTodayTimestamp()
    {
        return ToTodayTimestamp(DateTime.Now);
    }

    /// <summary>
    /// Unix 时间戳转本地时间
    /// </summary>
    /// <param name="milliseconds"></param>
    /// <returns></returns>
    public static DateTime UtcToDateTime(this long milliseconds)
    {
        var dt = UtcTime.AddMilliseconds(milliseconds);
        return dt;
    }

    /// <summary>
    /// 今日0点整时间戳转本地时间
    /// </summary>
    /// <param name="milliseconds"></param>
    /// <returns></returns>
    public static DateTime TodayToDateTime(this uint milliseconds)
    {
        var time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
        var dt = time.AddMilliseconds(milliseconds);
        return dt;
    }
}