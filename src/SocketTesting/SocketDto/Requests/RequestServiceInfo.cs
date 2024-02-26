namespace SocketDto.Requests;

/// <summary>
///     请求基本信息
/// </summary>
[NetHead(5, 1)]
[MessagePackObject]
public class RequestServiceInfo : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    [Key(0)]
    public int TaskId { get; set; }
}