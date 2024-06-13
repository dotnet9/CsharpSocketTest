using CodeWF.NetWeaver.Base;

namespace SocketNetObject;

public static class MessagePackHelper
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);


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

    public static T Deserialize<T>(byte[] buffer) where T : new()
    {
        var bodyBufferLen = buffer.Length - CodeWF.NetWeaver.SerializeHelper.PacketHeadLen;
        using var stream = new MemoryStream(buffer, CodeWF.NetWeaver.SerializeHelper.PacketHeadLen, bodyBufferLen);
        var data = MessagePackSerializer.Deserialize<T>(stream, Options);
        return data;
    }
}