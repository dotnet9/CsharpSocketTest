namespace SocketDto;

/// <summary>
///     TCP心跳包
/// </summary>
[NetHead(199, 1)]
public class Heartbeat : INetObject
{
}