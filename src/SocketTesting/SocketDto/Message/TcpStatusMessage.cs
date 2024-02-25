namespace SocketDto.Message;

/// <summary>
/// Socket连接状态
/// </summary>
/// <param name="sender"></param>
public class TcpStatusMessage(object sender, bool isConnect, string? ip = default, int port = default)
    : Messager.Message(sender)
{
    public string? Ip { get; } = ip;
    public int Port { get; } = port;
}