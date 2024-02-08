namespace SocketNetObject;

public partial class SerializeHelper
{
    public static T Deserialize<T>(this byte[] buffer) where T : new()
    {
        var bodyBufferLen = buffer.Length - PacketHeadLen;
        using var stream = new MemoryStream(buffer, PacketHeadLen, bodyBufferLen);
        var data = MessagePackSerializer.Deserialize<T>(stream, Options);
        return data;
    }
}