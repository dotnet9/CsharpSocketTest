namespace SocketDto.Requests;

/// <summary>
///     请求目标类型
/// </summary>
[NetHead(1, 1)]
[MessagePackObject]
public class RequestTargetType : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    [Key(0)]
    public int TaskId { get; set; }
}