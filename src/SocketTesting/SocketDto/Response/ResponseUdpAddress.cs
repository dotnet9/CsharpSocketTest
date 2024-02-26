namespace SocketDto.Response;

/// <summary>
///     响应Udp组播地址
/// </summary>
[NetHead(4, 1)]
[MessagePackObject]
public class ResponseUdpAddress : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    [Key(0)]
    public int TaskId { get; set; }


    /// <summary>
    ///     组播地址
    /// </summary>
    [Key(1)]
    public string? Ip { get; set; }

    /// <summary>
    ///     组播端口
    /// </summary>
    [Key(2)]
    public int Port { get; set; }
}