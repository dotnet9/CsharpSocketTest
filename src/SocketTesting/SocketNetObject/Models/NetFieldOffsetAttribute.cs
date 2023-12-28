namespace SocketNetObject.Models;

/// <summary>
/// 字段或属性bit配置
/// </summary>
/// <param name="size"></param>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NetFieldOffsetAttribute(byte offset, byte size) : Attribute
{
    /// <summary>
    /// 偏移
    /// </summary>
    public byte Offset { get; } = offset;

    /// <summary>
    /// 字段或属性bit大小
    /// </summary>
    public byte Size { get; } = size;
}