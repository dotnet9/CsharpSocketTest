namespace SocketDto;

/// <summary>
///     请求进程信息
/// </summary>
[NetHead(9, 1)]
public class RequestProcessList : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    public int TaskId { get; set; }
}