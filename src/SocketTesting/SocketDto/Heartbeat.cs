namespace SocketDto;

/// <summary>
/// TCP心跳包
/// </summary>
[NetHead(255, 1)]
[MessagePackObject]
public class Heartbeat : INetObject
{
    /// <summary>
    /// 已发送UDP包个数
    /// </summary>
    [Key(0)]
    public int UDPPacketsSentCount { get; set; }
}