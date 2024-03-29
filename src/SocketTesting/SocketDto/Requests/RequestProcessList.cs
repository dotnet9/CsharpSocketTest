﻿namespace SocketDto;

/// <summary>
///     请求进程信息
/// </summary>
[NetHead(9, 1)]
[MessagePackObject]
public class RequestProcessList : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    [Key(0)]
    public int TaskId { get; set; }
}