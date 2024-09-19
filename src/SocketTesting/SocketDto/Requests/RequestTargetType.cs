namespace SocketDto.Requests;

/// <summary>
///     请求目标类型
/// </summary>
[NetHead(1, 1)]
public class RequestTargetType : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    public int TaskId { get; set; }
}