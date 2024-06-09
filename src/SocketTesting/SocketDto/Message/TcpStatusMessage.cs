namespace SocketDto.Message;

/// <summary>
/// Tcp连接状态
/// </summary>
/// <param name="sender"></param>
public class TcpStatusMessage(object sender, bool isConnect, string? ip = default, int port = default)
    : CodeWF.EventBus.Message(sender)
{
    public bool IsConnect { get; } = isConnect;
    public string? Ip { get; } = ip;
    public int Port { get; } = port;
}