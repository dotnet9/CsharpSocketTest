using MessagePack;
using SocketNetObject;

namespace SocketDto;

/// <summary>
/// 操作系统进程信息
/// </summary>
public record ProcessItem
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

    private byte[]? _data;

    /// <summary>
    /// 见ProcessItemData
    /// </summary>
    public byte[]? Data
    {
        get => _data;
        set
        {
            _data = value;
            _processData = _data?.ToFieldObject<ProcessItemData>();
        }
    }

    private ProcessItemData? _processData;

    /// <summary>
    /// 进程数据
    /// </summary>
    [IgnoreMember]
    public ProcessItemData? ProcessData
    {
        get => _processData;
        set
        {
            _processData = value;
            _data = _processData?.FieldObjectBuffer();
        }
    }

    /// <summary>
    /// 上次更新时间（当天时间戳：当日0点0分0秒计算的时间戳，单位ms）
    /// </summary>
    public uint LastUpdateTime { get; set; }

    /// <summary>
    /// 更新时间（当天时间戳：当日0点0分0秒计算的时间戳，单位ms）
    /// </summary>
    public uint UpdateTime { get; set; }
}

/// <summary>
/// 进程数据信息
/// </summary>
public class ProcessItemData
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

    /// <summary>
    /// 占1bit，进程类型，0：应用，1：后台进程
    /// </summary>
    [NetFieldOffset(57, 1)]
    public byte Type { get; set; }

    /// <summary>
    /// 占1bit，进程状态，0：正常运行，1：效率模式，2：挂起
    /// </summary>
    [NetFieldOffset(58, 2)]
    public byte Status { get; set; }
}

public enum ProcessRunningStatus
{
    [Description("未运行")] None,
    [Description("已运行")] Running,
}

/// <summary>
/// 进程类型
/// </summary>
public enum ProcessType
{
    [Description("应用")] Application,
    [Description("后台进程")] BackgroundProcess
}

/// <summary>
/// 进程运行状态
/// </summary>
public enum ProcessStatus
{
    [Description("正常运行")] Running,
    [Description("效率模式")] EfficiencyMode,
    [Description("挂起")] Pending
}

/// <summary>
/// GPU引擎
/// </summary>
public enum GpuEngine
{
    [Description("无")] None,
    [Description("GPU 0 - 3D")] Gpu03D
}

/// <summary>
/// 电源使用情况
/// </summary>
public enum ProcessPowerUsage
{
    [Description("非常低")] VeryLow,
    [Description("低")] Low,
    [Description("中")] Moderate,
    [Description("高")] High,
    [Description("非常高")] VeryHigh
}