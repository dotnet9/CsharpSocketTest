namespace SocketTest.Common;

public static class TimestampHelper
{
    /// <summary>
    ///     获取从（2000+startYearFrom2000）-01-01 00:00:00到指定日期的时间戳（单位0.1s)
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="startYearFrom2000"></param>
    /// <returns></returns>
    public static uint ToTimestamp(this DateTime dateTime, byte startYearFrom2000)
    {
        var time = new DateTime(2020 + startYearFrom2000, 1, 1, 0, 0, 0, 0);
        var ts = dateTime - time;
        return (uint)(ts.TotalMilliseconds / 100);
    }

    /// <summary>
    ///     获取从（2000+startYearFrom2000）-01-01 00:00:00到现在的时间戳（单位ms)
    /// </summary>
    /// <returns></returns>
    public static uint GetCurrentTimestamp(this byte startYearFrom2000)
    {
        return ToTimestamp(DateTime.Now, startYearFrom2000);
    }


    /// <summary>
    ///     获取（2000+startYearFrom2000）-01-01 00:00:00起计算的时间戳转本地时间
    /// </summary>
    /// <param name="tenthOfSecond">时间戳，单位0.1秒</param>
    /// <param name="startYearFrom2000"></param>
    /// <returns></returns>
    public static DateTime ToDateTime(this uint tenthOfSecond, byte startYearFrom2000)
    {
        var time = new DateTime(2000 + startYearFrom2000, 1, 1, 0, 0, 0, 0);
        var dt = time.AddMilliseconds(tenthOfSecond * 100);
        return dt;
    }
}