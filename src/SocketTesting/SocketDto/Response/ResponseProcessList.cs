namespace SocketDto.Response;

/// <summary>
///     响应请求进程信息
/// </summary>
[NetHead(10, 1)]
public class ResponseProcessList : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    ///     总数据大小
    /// </summary>
    public int TotalSize { get; set; }

    /// <summary>
    ///     分页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    ///     总页数
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    ///     页索引
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    ///     进程列表
    /// </summary>
    public List<ProcessItem>? Processes { get; set; }
}

/// <summary>
///     操作系统进程信息
/// </summary>
public record ProcessItem
{
    /// <summary>
    ///     进程ID
    /// </summary>
    public int Pid { get; set; }

    /// <summary>
    ///     进程名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     进程类型，0：应用，1：后台进程
    /// </summary>
    public byte Type { get; set; }

    /// <summary>
    ///     进程状态，0：新建状态，1：就绪状态，2：运行状态，3：阻塞状态，4：终止状态
    /// </summary>
    public byte ProcessStatus { get; set; }

    /// <summary>
    ///     告警状态，没有特别意义，可组合位域状态，0：正常，1：超时，2：超限，切换用户
    /// </summary>
    public byte AlarmStatus { get; set; }

    /// <summary>
    ///     发布者
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    ///     命令行
    /// </summary>
    public string? CommandLine { get; set; }

    /// <summary>
    ///     Cpu（所有内核的总处理利用率），最后一位表示小数位，比如253表示25.3%
    /// </summary>
    public short Cpu { get; set; }

    /// <summary>
    ///     内存（进程占用的物理内存），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    public short Memory { get; set; }

    /// <summary>
    ///     磁盘（所有物理驱动器的总利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    public short Disk { get; set; }

    /// <summary>
    ///     网络（当前主要网络上的网络利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    public short Network { get; set; }

    /// <summary>
    ///     GPU(所有GPU引擎的最高利用率)，最后一位表示小数位，比如253表示25.3
    /// </summary>
    public short Gpu { get; set; }

    /// <summary>
    ///     GPU引擎，0：无，1：GPU 0 - 3D
    /// </summary>
    public byte GpuEngine { get; set; }

    /// <summary>
    ///     电源使用情况（CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
    /// </summary>
    public byte PowerUsage { get; set; }

    /// <summary>
    ///     电源使用情况趋势（一段时间内CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
    /// </summary>
    public byte PowerUsageTrend { get; set; }

    /// <summary>
    ///     上次更新时间（当天时间戳：当日0点0分0秒计算的时间戳，单位ms）
    /// </summary>
    public uint LastUpdateTime { get; set; }

    /// <summary>
    ///     更新时间（当天时间戳：当日0点0分0秒计算的时间戳，单位ms）
    /// </summary>
    public uint UpdateTime { get; set; }
}