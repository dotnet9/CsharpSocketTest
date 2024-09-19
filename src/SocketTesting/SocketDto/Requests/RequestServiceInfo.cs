namespace SocketDto.Requests;

/// <summary>
///     请求基本信息
/// </summary>
[NetHead(5, 1)]
public class RequestServiceInfo : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    public int TaskId { get; set; }
}