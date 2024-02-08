namespace SocketNetObject;

public partial class SerializeHelper
{
    public static byte[] Serialize<T>(this T data, long systemId) where T : INetObject
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        var netObjectInfo = GetNetObjectHead(data.GetType());
        dynamic netObject = data;
        var bodyBuffer = MessagePackSerializer.Serialize(netObject, Options);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, DefaultEncoding);

        writer.Write(PacketHeadLen + bodyBuffer.Length);
        writer.Write(systemId);
        writer.Write(netObjectInfo.Id);
        writer.Write(netObjectInfo.Version);
        writer.Write(bodyBuffer);

        return stream.ToArray();
    }
}