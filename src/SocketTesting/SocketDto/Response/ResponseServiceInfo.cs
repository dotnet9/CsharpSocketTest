namespace SocketDto.Response;

/// <summary>
///     响应基本信息
/// </summary>
[NetHead(6, 1)]
[MessagePackObject]
public class ResponseServiceInfo : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    [Key(0)]
    public int TaskId { get; set; }

    /// <summary>
    ///     操作系统名称
    /// </summary>
    [Key(1)]
    public string? OS { get; set; }

    /// <summary>
    ///     系统内存大小（单位GB）
    /// </summary>
    [Key(2)]
    public byte MemorySize { get; set; }

    /// <summary>
    ///     处理器个数
    /// </summary>
    [Key(3)]
    public byte ProcessorCount { get; set; }

    /// <summary>
    ///     硬盘总容量（单位GB）
    /// </summary>
    [Key(4)]
    public short DiskSize { get; set; }

    /// <summary>
    ///     网络带宽（单位Mbps）
    /// </summary>
    [Key(5)]
    public short NetworkBandwidth { get; set; }

    /// <summary>
    ///     服务器IP地址，多个地址以“，”分隔
    /// </summary>
    [Key(6)]
    public string? Ips { get; set; }

    /// <summary>
    ///     通信对象时间戳起始年份，比如：2023，表示2023年1月1号开始计算时间戳，后面的时间戳都以这个字段计算为准，精确到0.1s，即100ms，主要用于节约网络对象传输大小
    /// </summary>
    [Key(7)]
    public int TimestampStartYear { get; set; }

    /// <summary>
    ///     最后更新时间
    /// </summary>
    [Key(8)]
    public uint LastUpdateTime { get; set; }
}