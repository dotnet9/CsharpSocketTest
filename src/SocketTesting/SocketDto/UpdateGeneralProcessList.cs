namespace SocketDto;

/// <summary>
///     更新进程变化信息，序列化和反序列不能加压缩，部分双精度因为有效位数太长，可能导致UDP包过大而发送失败，所以UDP包不要加压缩
/// </summary>
[NetHead(201, 1)]
public class UpdateGeneralProcessList : INetObject
{
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
    public List<GeneralProcessItem>? Processes { get; set; }
}

/// <summary>
///     操作系统进程信息
/// </summary>
public class GeneralProcessItem
{
    /// <summary>
    ///     对象大小，Data字段为3字节，序列化时需要3字节表示byte[]长度，所有3个序列化字段总大小为 sizeof(byte)+ (4 + 3) + sizeof(uint) = 12
    /// </summary>
    public const int ObjectSize = sizeof(byte) + (sizeof(int) + 3) + sizeof(uint);

    #region 网络传输字段

    private byte _status;

    public byte Status
    {
        get => _status;
        set
        {
            _status = value;
            _processStatus = (ProcessOtherStatus)Enum.Parse(typeof(ProcessOtherStatus), value.ToString());
        }
    }

    /// <summary>
    ///     见ActiveProcessData定义
    /// </summary>
    private byte[]? _data;


    /// <summary>
    ///     见ActiveProcessItemData
    /// </summary>
    public byte[]? Data
    {
        get => _data;
        set
        {
            _data = value;
            _processData = _data?.ToFieldObject<GeneralProcessItemData>();
        }
    }

    /// <summary>
    /// 更新时间（当天时间戳：当日0点0分0秒计算的时间戳，单位ms）
    /// </summary>
    public uint UpdateTime { get; set; }

    #endregion

    #region 编程字段数据转换辅助字段

    private ProcessOtherStatus _processStatus;

    [NetIgnoreMember]
    public ProcessOtherStatus ProcessStatus
    {
        get => _processStatus;
        set
        {
            _processStatus = value;
            _status = (byte)value;
        }
    }

    private GeneralProcessItemData? _processData;

    /// <summary>
    ///     进程数据
    /// </summary>
    [NetIgnoreMember]
    public GeneralProcessItemData? ProcessData
    {
        get => _processData;
        set
        {
            _processData = value;
            _data = _processData?.FieldObjectBuffer();
        }
    }

    #endregion
}

public record GeneralProcessItemData
{
    /// <summary>
    ///     占10bit, GPU(所有GPU引擎的最高利用率)，最后一位表示小数位，比如253表示25.3
    /// </summary>
    [NetFieldOffset(0, 10)]
    public short Gpu { get; set; }

    /// <summary>
    ///     占1bit，GPU引擎，0：无，1：GPU 0 - 3D
    /// </summary>
    [NetFieldOffset(10, 1)]
    public byte GpuEngine { get; set; }

    /// <summary>
    ///     占3bit，电源使用情况（CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
    /// </summary>
    [NetFieldOffset(11, 3)]
    public byte PowerUsage { get; set; }

    /// <summary>
    ///     占3bit，电源使用情况趋势（一段时间内CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
    /// </summary>
    [NetFieldOffset(14, 3)]
    public byte PowerUsageTrend { get; set; }

    public override string ToString()
    {
        return
            $"{nameof(Gpu)}={Gpu}，{nameof(GpuEngine)}={GpuEngine}，{nameof(PowerUsage)}={PowerUsage}，{nameof(PowerUsageTrend)}={PowerUsageTrend}";
    }
}

/// <summary>
/// 进程状态（没有意思，只用于测试枚举位域使用）
/// </summary>
[Flags]
public enum ProcessOtherStatus
{
    [Description("正常")] Normal = 0,
    [Description("超时")] Overtime = 1,
    [Description("超限")] OverLimit = 2,
    [Description("切换用户")] UserChanged = 4
}