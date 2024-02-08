using System.Net.Sockets;

namespace SocketDto.Message;

/// <summary>
/// Socket信息，收到其他端网络对象后转换为此对象，在进程之内传递
/// </summary>
/// <param name="sender"></param>
/// <param name="netHead"></param>
/// <param name="buffer"></param>
/// <param name="client"></param>
public class SocketMessage(object sender, NetHeadInfo netHead, byte[] buffer, Socket? client = null)
    : Messager.Message(sender)
{
    /// <summary>
    /// 数据包头部信息
    /// </summary>
    private NetHeadInfo HeadInfo { get; } = netHead;

    /// <summary>
    /// 数据
    /// </summary>
    private byte[] Buffer { get; } = buffer;

    /// <summary>
    /// Socket对象
    /// </summary>
    public Socket? Client { get; } = client;

    /// <summary>
    /// 判断是否是指定网络对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool IsMessage<T>()
    {
        return HeadInfo.IsNetObject<T>();
    }

    /// <summary>
    /// 使用MessagePack反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Message<T>() where T : new()
    {
        return Buffer.Deserialize<T>();
    }

    /// <summary>
    /// 不使用压缩方式反序列化，UDP数据包使用该方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T MessageByNative<T>() where T : new()
    {
        return Buffer.DeserializeByNative<T>();
    }
}