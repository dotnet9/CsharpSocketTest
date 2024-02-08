namespace SocketDto;

/// <summary>
///     响应请求进程信息
/// </summary>
[NetHead(4, 1)]
[MessagePackObject]
public class ResponseProcessList : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    [Key(0)]
    public int TaskId { get; set; }

    /// <summary>
    ///     总数据大小
    /// </summary>
    [Key(1)]
    public int TotalSize { get; set; }

    /// <summary>
    ///     分页大小
    /// </summary>
    [Key(2)]
    public int PageSize { get; set; }

    /// <summary>
    ///     总页数
    /// </summary>
    [Key(3)]
    public int PageCount { get; set; }

    /// <summary>
    ///     页索引
    /// </summary>
    [Key(4)]
    public int PageIndex { get; set; }

    /// <summary>
    ///     进程列表
    /// </summary>
    [Key(5)]
    public List<ProcessItem>? Processes { get; set; }
}

/// <summary>
///     操作系统进程信息
/// </summary>
[MessagePackObject]
public record ProcessItem
{
    private byte[]? _data;

    private ProcessItemData? _processData;

    #region 网络通信字段

    /// <summary>
    ///     进程ID
    /// </summary>
    [Key(0)]
    public int PID { get; set; }

    /// <summary>
    ///     进程名称
    /// </summary>
    [Key(1)]
    public string? Name { get; set; }

    /// <summary>
    ///     发布者
    /// </summary>
    [Key(2)]
    public string? Publisher { get; set; }

    /// <summary>
    ///     命令行
    /// </summary>
    [Key(3)]
    public string? CommandLine { get; set; }

    /// <summary>
    ///     见ProcessItemData
    /// </summary>
    [Key(4)]
    public byte[]? Data
    {
        get => _data;
        set
        {
            _data = value;
            _processData = _data?.ToFieldObject<ProcessItemData>();
        }
    }

    /// <summary>
    ///     上次更新时间（当天时间戳：当日0点0分0秒计算的时间戳，单位ms）
    /// </summary>
    [Key(5)]
    public uint LastUpdateTime { get; set; }

    /// <summary>
    ///     更新时间（当天时间戳：当日0点0分0秒计算的时间戳，单位ms）
    /// </summary>
    [Key(6)]
    public uint UpdateTime { get; set; }

    #endregion

    /// <summary>
    ///     进程数据
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
}

/// <summary>
///     进程数据信息
/// </summary>
public class ProcessItemData
{
    #region 网络通信字段

    /// <summary>
    ///     占10bit, CPU（所有内核的总处理利用率），最后一位表示小数位，比如253表示25.3%
    /// </summary>
    [NetFieldOffset(0, 10)]
    public short Cpu { get; set; }

    /// <summary>
    ///     占10bit, 内存（进程占用的物理内存），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    [NetFieldOffset(10, 10)]
    public short Memory { get; set; }

    /// <summary>
    ///     占10bit, 磁盘（所有物理驱动器的总利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    [NetFieldOffset(20, 10)]
    public short Disk { get; set; }

    /// <summary>
    ///     占10bit, 网络（当前主要网络上的网络利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
    /// </summary>
    [NetFieldOffset(30, 10)]
    public short Network { get; set; }

    /// <summary>
    ///     占10bit, GPU(所有GPU引擎的最高利用率)，最后一位表示小数位，比如253表示25.3
    /// </summary>
    [NetFieldOffset(40, 10)]
    public short Gpu { get; set; }

    private byte _gpuEngine;

    /// <summary>
    ///     占1bit，GPU引擎，0：无，1：GPU 0 - 3D
    /// </summary>
    [NetFieldOffset(50, 1)]
    public byte GpuEngine
    {
        get => _gpuEngine;
        set
        {
            _gpuEngine = value;
            _gpuEngineKind = (GpuEngine)Enum.Parse(typeof(GpuEngine), value.ToString());
        }
    }

    private byte _powerUsage;

    /// <summary>
    ///     占3bit，电源使用情况（CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
    /// </summary>
    [NetFieldOffset(51, 3)]
    public byte PowerUsage
    {
        get => _powerUsage;
        set
        {
            _powerUsage = value;
            _powerUsageKind = (ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), value.ToString());
        }
    }

    private byte _powerUsageTrend;

    /// <summary>
    ///     占3bit，电源使用情况趋势（一段时间内CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
    /// </summary>
    [NetFieldOffset(54, 3)]
    public byte PowerUsageTrend
    {
        get => _powerUsageTrend;
        set
        {
            _powerUsageTrend = value;
            _powerUsageTrendKind = (ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), value.ToString());
        }
    }

    private byte _type;

    /// <summary>
    ///     占1bit，进程类型，0：应用，1：后台进程
    /// </summary>
    [NetFieldOffset(57, 1)]
    public byte Type
    {
        get => _type;
        set
        {
            _type = value;
            _typeKind = (ProcessType)Enum.Parse(typeof(ProcessType), value.ToString());
        }
    }

    private byte _status;

    /// <summary>
    ///     占3bit，进程状态，0：新建状态，1：就绪状态，2：运行状态，3：阻塞状态，4：终止状态
    /// </summary>
    [NetFieldOffset(58, 3)]
    public byte Status
    {
        get => _status;
        set
        {
            _status = value;
            _statusKind = (ProcessStatus)Enum.Parse(typeof(ProcessStatus), value.ToString());
        }
    }

    #endregion

    #region 程序处理字段

    private GpuEngine _gpuEngineKind;

    /// <summary>
    ///     GPU引擎
    /// </summary>
    [IgnoreMember]
    public GpuEngine GpuEngineKind
    {
        get => _gpuEngineKind;
        set
        {
            _gpuEngineKind = value;
            GpuEngine = (byte)value;
        }
    }

    private ProcessPowerUsage _powerUsageKind;

    /// <summary>
    ///     电源使用情况
    /// </summary>
    [IgnoreMember]
    public ProcessPowerUsage PowerUsageKind
    {
        get => _powerUsageKind;
        set
        {
            _powerUsageKind = value;
            PowerUsage = (byte)value;
        }
    }

    private ProcessPowerUsage _powerUsageTrendKind;

    /// <summary>
    ///     电源使用情况趋势
    /// </summary>
    [IgnoreMember]
    public ProcessPowerUsage PowerUsageTrendKind
    {
        get => _powerUsageTrendKind;
        set
        {
            _powerUsageTrendKind = value;
            _powerUsageTrend = (byte)value;
        }
    }

    private ProcessType _typeKind;

    /// <summary>
    ///     进程类型
    /// </summary>
    [IgnoreMember]
    public ProcessType TypeKind
    {
        get => _typeKind;
        set
        {
            _typeKind = value;
            _type = (byte)value;
        }
    }

    private ProcessStatus _statusKind;

    /// <summary>
    ///     进程状态
    /// </summary>
    [IgnoreMember]
    public ProcessStatus StatusKind
    {
        get => _statusKind;
        set
        {
            _statusKind = value;
            _status = (byte)value;
        }
    }

    #endregion
}

/// <summary>
///     进程类型
/// </summary>
public enum ProcessType
{
    [Description("应用")] Application,
    [Description("后台进程")] BackgroundProcess
}

/// <summary>
///     进程运行状态
/// </summary>
public enum ProcessStatus
{
    [Description("新建状态")] New,
    [Description("就绪状态")] Ready,
    [Description("运行状态")] Running,
    [Description("阻塞状态")] Blocked,
    [Description("终止状态")] Terminated
}

/// <summary>
///     GPU引擎
/// </summary>
public enum GpuEngine
{
    [Description("无")] None,
    [Description("GPU 0 - 3D")] Gpu03D
}

/// <summary>
///     电源使用情况
/// </summary>
public enum ProcessPowerUsage
{
    [Description("非常低")] VeryLow,
    [Description("低")] Low,
    [Description("中")] Moderate,
    [Description("高")] High,
    [Description("非常高")] VeryHigh
}