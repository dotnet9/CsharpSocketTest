namespace SocketNetObject;

public partial class MessagePackHelper
{
    public static T Deserialize<T>(byte[] buffer) where T : new()
    {
        var bodyBufferLen = buffer.Length - CodeWF.NetWeaver.SerializeHelper.PacketHeadLen;
        using var stream = new MemoryStream(buffer, CodeWF.NetWeaver.SerializeHelper.PacketHeadLen, bodyBufferLen);
        var data = MessagePackSerializer.Deserialize<T>(stream, Options);
        return data;
    }
}