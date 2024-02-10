namespace SocketDto;

/// <summary>
///     TCP心跳包
/// </summary>
[NetHead(199, 1)]
[MessagePackObject]
public class Heartbeat : INetObject
{
}