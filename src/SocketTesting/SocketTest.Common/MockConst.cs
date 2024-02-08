namespace SocketTest.Common;

public class MockConst
{
    /// <summary>
    ///     服务端更新UDP数据间隔，单位ms
    /// </summary>
    public const int UdpUpdateMilliseconds = 500;

    /// <summary>
    ///     服务端发送UDP实时数据间隔，单位ms
    /// </summary>
    public const int UdpSendRealtimeMilliseconds = 500;

    /// <summary>
    ///     服务端发送UDP一般数据间隔，单位ms
    /// </summary>
    public const int UdpSendGeneralMilliseconds = 1000;

    /// <summary>
    ///     客户端处理UDP数据间隔，单位ms
    /// </summary>
    public const int UdpDillMilliseconds = 150;
}