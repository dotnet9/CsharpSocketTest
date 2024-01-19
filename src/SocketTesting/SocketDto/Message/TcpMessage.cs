namespace SocketDto.Message;

public class TcpMessage(object sender, INetObject netObject) : Messager.Message(sender)
{
    public INetObject NetObject { get; set; } = netObject;
}