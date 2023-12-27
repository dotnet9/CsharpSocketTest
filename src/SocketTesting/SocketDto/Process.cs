namespace SocketDto;

/// <summary>
/// 操作系统进程信息
/// </summary>
public record Process
{
    /// <summary>
    /// 进程ID
    /// </summary>
    public int PID { get; set; }

    /// <summary>
    /// 进程名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 发布者
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// 命令行
    /// </summary>
    public string? CommandLine { get; set; }

    /// <summary>
    /// 见ProcessData
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    /// 上次更新时间（当天时间戳：当日0点0分0秒计算的时间戳，单位ms）
    /// </summary>
    public uint LastUpdateTime { get; set; }

    /// <summary>
    /// 更新时间（当天时间戳：当日0点0分0秒计算的时间戳，单位ms）
    /// </summary>
    public uint UpdateTime { get; set; }
}

public record ProcessData
{
    /// <summary>
    /// 占10bit, CPU（所有内核的总处理利用率），最后一位表示小数位，比如253表示25.3%
    /// </summary>
    public short Cpu { get; set; }

    /// <summary>
    /// 占10bit, 内存（进程占用的物理内存），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    public short Memory { get; set; }

    /// <summary>
    /// 占10bit, 磁盘（所有物理驱动器的总利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    public short Disk { get; set; }

    /// <summary>
    /// 占10bit, 网络（当前主要网络上的网络利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    public short Network { get; set; }

    /// <summary>
    /// 占10bit, GPU(所有GPU引擎的最高利用率)，最后一位表示小数位，比如253表示25.3
    /// </summary>
    public short Gpu { get; set; }

    /// <summary>
    /// 占1bit，GPU引擎，0：无，1：GPU 0 - 3D
    /// </summary>
    public byte GpuEngine { get; set; }

    /// <summary>
    /// 占3bit，电源使用情况（CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
    /// </summary>
    public byte PowerUsage { get; set; }

    /// <summary>
    /// 占3bit，电源使用情况趋势（一段时间内CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
    /// </summary>
    public byte PowerUsageTrend { get; set; }

    /// <summary>
    /// 占1bit，进程类型，0：应用，1：后台进程
    /// </summary>
    public byte Type { get; set; }

    /// <summary>
    /// 占1bit，进程状态，0：正常运行，1：效率模式，2：挂起
    /// </summary>
    public byte Status { get; set; }
}