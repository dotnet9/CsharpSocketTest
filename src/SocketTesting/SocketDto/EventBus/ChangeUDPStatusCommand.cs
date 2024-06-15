namespace SocketDto.EventBus;

/// <summary>
/// Udp连接状态
/// </summary>
public class ChangeUDPStatusCommand(bool isConnect) : Command
{
    public bool IsConnect { get; } = isConnect;
}