using CodeWF.NetWeaver.Base;

namespace SocketNetObject;

public partial class MessagePackHelper
{
    public static byte[] Serialize<T>(T data, long systemId) where T : INetObject
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        var netObjectInfo = CodeWF.NetWeaver.SerializeHelper.GetNetObjectHead(data.GetType());
        dynamic netObject = data;
        var bodyBuffer = MessagePackSerializer.Serialize(netObject, Options);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, CodeWF.NetWeaver.SerializeHelper.DefaultEncoding);

        writer.Write(CodeWF.NetWeaver.SerializeHelper.PacketHeadLen + bodyBuffer.Length);
        writer.Write(systemId);
        writer.Write(netObjectInfo.Id);
        writer.Write(netObjectInfo.Version);
        writer.Write(bodyBuffer);

        return stream.ToArray();
    }
}