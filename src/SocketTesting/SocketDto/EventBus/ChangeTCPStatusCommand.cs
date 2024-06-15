namespace SocketDto.EventBus;

/// <summary>
/// Tcp连接状态
/// </summary>
public class ChangeTCPStatusCommand(bool isConnect, string? ip = default, int port = default)
    : Command
{
    public bool IsConnect { get; } = isConnect;
    public string? Ip { get; } = ip;
    public int Port { get; } = port;
}