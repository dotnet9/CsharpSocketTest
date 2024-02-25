namespace SocketDto.Udp;

/// <summary>
///     更新进程变化信息，序列化和反序列不能加压缩，部分双精度因为有效位数太长，可能导致UDP包过大而发送失败，所以UDP包不要加压缩
/// </summary>
[NetHead(200, 1)]
public class UpdateRealtimeProcessList : INetObject
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
    /// 一个进程占2字节(short)
    /// </summary>
    public byte[] Cpus { get; set; } = null!;

    /// <summary>
    ///     一个进程占2字节(short)
    /// </summary>
    [NetFieldOffset(10, 10)]
    public byte[] Memories { get; set; } = null!;

    /// <summary>
    ///     一个进程占2字节(short)
    /// </summary>
    [NetFieldOffset(20, 10)]
    public byte[] Disks { get; set; } = null!;

    /// <summary>
    ///     一个进程占2字节(short)
    /// </summary>
    [NetFieldOffset(30, 10)]
    public byte[] Networks { get; set; } = null!;
}