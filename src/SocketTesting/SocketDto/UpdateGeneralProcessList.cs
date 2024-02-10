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
    ///     进程状态，一个进程占1字节(byte)
    /// </summary>
    public byte[] ProcessStatuses { get; set; } = null!;

    /// <summary>
    ///     告警状态，一个进程占1字节(byte)
    /// </summary>
    public byte[] AlarmStatuses { get; set; } = null!;

    /// <summary>
    ///     一个进程占2字节(short)
    /// </summary>
    public byte[] Gpus { get; set; } = null!;

    /// <summary>
    ///     一个进程占1字节(byte)
    /// </summary>
    public byte[] GpuEngine { get; set; } = null!;

    /// <summary>
    ///     一个进程占1字节(byte)
    /// </summary>
    public byte[] PowerUsage { get; set; } = null!;

    /// <summary>
    ///     一个进程占1字节(byte)
    /// </summary>
    public byte[] PowerUsageTrend { get; set; } = null!;

    /// <summary>
    ///     一个进程占4字节(byte)
    /// </summary>
    public byte[] UpdateTimes { get; set; } = null!;
}