namespace SocketDto.Requests;

/// <summary>
///     请求进程ID列表信息
/// </summary>
[NetHead(7, 1)]
public class RequestProcessIDList : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    public int TaskId { get; set; }
}