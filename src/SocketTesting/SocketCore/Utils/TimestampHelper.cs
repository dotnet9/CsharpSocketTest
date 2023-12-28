namespace SocketCore.Utils;

public static class TimestampHelper
{
    /// <summary>
    /// Utc时间戳计算起始时间
    /// </summary>
    private static readonly DateTime UtcTime = new(1970, 1, 1, 0, 0, 0, 0);

    /// <summary>
    /// 获取UTC时间戳（单位ms)
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static long ToUtcTimestamp(this DateTime dateTime)
    {
        var ts = dateTime - UtcTime;
        return (long)ts.TotalMilliseconds;
    }

    /// <summary>
    /// 获取今天0点到此刻的时间戳（单位ms)
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static uint ToTodayTimestamp(this DateTime dateTime)
    {
        var time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
        var ts = dateTime - time;
        return (uint)ts.TotalMilliseconds;
    }

    /// <summary>
    /// 获取今天0点到此刻的时间戳（单位ms)
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="startYearFrom2000"></param>
    /// <returns></returns>
    public static uint ToTimestamp(this DateTime dateTime, byte startYearFrom2000)
    {
        var time = new DateTime(2020 + startYearFrom2000, 1, 1, 0, 0, 0, 0);
        var ts = dateTime - time;
        return (uint)ts.TotalMilliseconds;
    }


    /// <summary>
    /// 获取当前UTC时间戳
    /// </summary>
    /// <returns></returns>
    public static long GetCurrentUtcTimestamp()
    {
        return ToUtcTimestamp(DateTime.Now);
    }

    /// <summary>
    /// 获取今天到现在的时间戳（单位ms)
    /// </summary>
    /// <returns></returns>
    public static uint GetCurrentTodayTimestamp()
    {
        return ToTodayTimestamp(DateTime.Now);
    }

    /// <summary>
    /// 获取今天到现在的时间戳（单位ms)
    /// </summary>
    /// <returns></returns>
    public static uint GetCurrentTimestamp(this byte startYearFrom2000)
    {
        return ToTimestamp(DateTime.Now, startYearFrom2000);
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

    /// <summary>
    /// 今日0点整时间戳转本地时间
    /// </summary>
    /// <param name="milliseconds"></param>
    /// <param name="startYearFrom2000"></param>
    /// <returns></returns>
    public static DateTime ToDateTime(this uint milliseconds, byte startYearFrom2000)
    {
        var time = new DateTime(2000 + startYearFrom2000, 1, 1, 0, 0, 0, 0);
        var dt = time.AddMilliseconds(milliseconds);
        return dt;
    }
}