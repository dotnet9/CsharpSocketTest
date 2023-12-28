namespace SocketDto;

/// <summary>
/// TCP心跳包
/// </summary>
[NetHead(255, 1)]
public class Heartbeat : INetObject
{
}