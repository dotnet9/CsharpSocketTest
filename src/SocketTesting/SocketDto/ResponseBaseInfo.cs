﻿namespace SocketDto;

/// <summary>
/// 响应基本信息
/// </summary>
[NetObjectHead(2, 1)]
public class ResponseBaseInfo : INetObject
{
    /// <summary>
    /// 任务Id
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// 服务器操作系统类型
    /// </summary>
    public string? OperatingSystemType { get; set; }

    /// <summary>
    /// 系统内存大小（单位GB）
    /// </summary>
    public byte MemorySize { get; set; }

    /// <summary>
    /// 处理器个数
    /// </summary>
    public byte ProcessorCount { get; set; }

    /// <summary>
    /// 硬盘总容量（单位GB）
    /// </summary>
    public short TotalDiskSpace { get; set; }

    /// <summary>
    /// 网络带宽（单位Mbps）
    /// </summary>
    public short NetworkBandwidth { get; set; }

    /// <summary>
    /// 服务器IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 服务器名称
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// 数据中心位置
    /// </summary>
    public string? DataCenterLocation { get; set; }

    /// <summary>
    /// 运行状态
    /// </summary>
    public byte IsRunning { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public uint LastUpdateTime { get; set; }
}