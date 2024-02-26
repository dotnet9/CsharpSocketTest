namespace SocketDto.Message;

/// <summary>
/// Udp连接状态
/// </summary>
/// <param name="sender"></param>
public class UdpStatusMessage(object sender, bool isConnect)
    : Messager.Message(sender)
{
    public bool IsConnect { get; } = isConnect;
}