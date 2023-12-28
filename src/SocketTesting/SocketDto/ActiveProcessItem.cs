namespace SocketDto;

/// <summary>
/// 操作系统进程信息
/// </summary>
public class ActiveProcessItem
{
    /// <summary>
    /// 见ActiveProcessData定义
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    /// 更新时间（当天时间戳：当日0点0分0秒计算的时间戳，单位ms）
    /// </summary>
    public uint UpdateTime { get; set; }
}

public record ActiveProcessItemData
{
    /// <summary>
    /// 占10bit, CPU（所有内核的总处理利用率），最后一位表示小数位，比如253表示25.3%
    /// </summary>
    [NetFieldOffset(0, 10)]
    public short Cpu { get; set; }

    /// <summary>
    /// 占10bit, 内存（进程占用的物理内存），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    [NetFieldOffset(10, 10)]
    public short Memory { get; set; }

    /// <summary>
    /// 占10bit, 磁盘（所有物理驱动器的总利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    [NetFieldOffset(20, 10)]
    public short Disk { get; set; }

    /// <summary>
    /// 占10bit, 网络（当前主要网络上的网络利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    [NetFieldOffset(30, 10)]
    public short Network { get; set; }

    /// <summary>
    /// 占10bit, GPU(所有GPU引擎的最高利用率)，最后一位表示小数位，比如253表示25.3
    /// </summary>
    [NetFieldOffset(40, 10)]
    public short Gpu { get; set; }

    /// <summary>
    /// 占1bit，GPU引擎，0：无，1：GPU 0 - 3D
    /// </summary>
    [NetFieldOffset(50, 1)]
    public byte GpuEngine { get; set; }

    /// <summary>
    /// 占3bit，电源使用情况（CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
    /// </summary>
    [NetFieldOffset(51, 3)]
    public byte PowerUsage { get; set; }

    /// <summary>
    /// 占3bit，电源使用情况趋势（一段时间内CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
    /// </summary>
    [NetFieldOffset(54, 3)]
    public byte PowerUsageTrend { get; set; }
}