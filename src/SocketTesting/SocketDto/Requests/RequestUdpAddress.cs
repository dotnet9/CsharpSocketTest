namespace SocketDto.Requests;

/// <summary>
///     请求Udp组播地址
/// </summary>
[NetHead(3, 1)]
[MessagePackObject]
public class RequestUdpAddress : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    [Key(0)]
    public int TaskId { get; set; }
}