namespace SocketDto.Response;

/// <summary>
///     响应请求进程ID列表信息
/// </summary>
[NetHead(8, 1)]
public class ResponseProcessIDList : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    ///     进程ID数组，有顺序，更新进程实时数据包需要根据该数组查找进程、更新数据
    /// </summary>
    public int[]? IDList { get; set; }
}