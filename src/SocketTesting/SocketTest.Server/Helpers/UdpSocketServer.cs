using CodeWF.Log.Core;
using CodeWF.NetWeaver;
using CodeWF.NetWeaver.Base;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SocketTest.Server.Helpers;

/// <summary>
/// UDP Socket 服务端类，用于创建UDP组播服务器并发送数据
/// </summary>
public class UdpSocketServer
{
    /// <summary>
    /// UDP客户端对象
    /// </summary>
    private UdpClient? _client;

    /// <summary>
    /// UDP IP端点
    /// </summary>
    private IPEndPoint? _udpIpEndPoint;


    #region 公开属性

    /// <summary>
    /// 服务标识，用以区分多个服务
    /// </summary>
    public string? ServerMark { get; private set; }
    /// <summary>
    /// 获取或设置服务器IP地址
    /// </summary>
    public string? ServerIP { get; private set; }

    /// <summary>
    /// 获取或设置服务器端口号
    /// </summary>
    public int ServerPort { get; private set; }

    /// <summary>
    /// 获取或设置系统ID
    /// </summary>
    public long SystemId { get; private set; }
    /// <summary>
    /// 获取或设置回环IP地址
    /// </summary>
    public static string LoopbackIP { get; set; } = "127.0.0.1";

    /// <summary>
    /// 获取或设置回环子IP地址
    /// </summary>
    public static string LoopbackSubIP { get; set; } = "239.0.0.1";


    /// <summary>
    ///     是否正在运行udp组播订阅
    /// </summary>
    public bool IsRunning { get; set; }

    #endregion

    #region 公开接口方法


    /// <summary>
    /// 启动UDP组播服务器
    /// </summary>
    /// <param name="serverMark">服务器标识</param>
    /// <param name="systemId">系统ID</param>
    /// <param name="serverIP">服务器IP地址</param>
    /// <param name="serverPort">服务器端口号</param>
    /// <param name="localIP">本地IP地址</param>
    /// <returns>启动结果和错误信息</returns>
    public (bool IsSuccess, string? ErrorMessage) Start(string serverMark, long systemId, string serverIP, int serverPort, string localIP = "0.0.0.0")
    {
        ServerMark = serverMark;
        ServerIP = serverIP;
        ServerPort = serverPort;
        SystemId = systemId;

        try
        {
            var localNic = IPAddress.Parse(localIP);
            _client = new UdpClient();

            // 允许复用端口（可选）
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // 绑定到指定网卡（非常关键）
            _client.Client.Bind(new IPEndPoint(localNic, 0));

            // 设置组播 TTL（本地网段）
            _client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1);

            // * 设置发送使用的网卡接口
            _client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, localNic.GetAddressBytes());

            // 开启回环
            _client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);

            _udpIpEndPoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);

            IsRunning = true;

            Logger.Info($"{ServerIP} 组播启动成功，组播地址：{ServerIP}:{ServerPort}");
            return (IsSuccess: true, ErrorMessage: null);
        }
        catch (Exception ex)
        {
            IsRunning = false;
            Logger.Error($"{ServerIP} 组播启动失败，组播地址：{ServerIP}:{ServerPort}", ex, $"{ServerIP} 组播启动失败，组播地址：{ServerIP}:{ServerPort}，详细信息请查看日志文件");
            return (IsSuccess: false, ErrorMessage: $"{ServerIP} 组播启动失败，组播地址：{ServerIP}:{ServerPort}，异常信息：{ex.Message}");
        }
    }

    /// <summary>
    /// 停止UDP组播服务器
    /// </summary>
    public void Stop()
    {
        try
        {
            _client?.Close();
            _client = null;
            Logger.Info($"{ServerIP} 组播停止");
        }
        catch (Exception ex)
        {
            Logger.Error($"{ServerIP} 组播停止Udp异常", ex, $"{ServerIP} 组播停止Udp异常，详细信息请查看日志文件");
        }

        IsRunning = false;
    }

    /// <summary>
    /// 发送命令
    /// </summary>
    /// <param name="command">要发送的网络对象命令</param>
    /// <param name="time">发送时间</param>
    public async Task SendCommandAsync(INetObject command, DateTimeOffset time)
    {
        if (!IsRunning || _client == null) return;

        var buffer = command.Serialize(SystemId, time);
        var sendCount = await _client.SendAsync(buffer, buffer.Length, _udpIpEndPoint);
        if (sendCount < buffer.Length)
        {
            Console.WriteLine($"UDP发送失败一包：{buffer.Length}=>{sendCount}");
        }
    }

    #endregion
}