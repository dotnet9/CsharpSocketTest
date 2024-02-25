namespace SocketDto.Requests;

/// <summary>
///     请求进程ID列表信息
/// </summary>
[NetHead(5, 1)]
[MessagePackObject]
public class RequestProcessIDList : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    [Key(0)]
    public int TaskId { get; set; }
}