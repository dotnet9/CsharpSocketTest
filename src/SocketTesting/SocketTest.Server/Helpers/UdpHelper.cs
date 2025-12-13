using CodeWF.Log.Core;
using CodeWF.NetWeaver;
using CodeWF.NetWeaver.Base;
using ReactiveUI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest.Server.Helpers;

public class UdpHelper(TcpHelper tcpHelper) : ReactiveObject
{
    private UdpClient? _client;
    private IPEndPoint? _udpIpEndPoint;

    #region 公开属性

    /// <summary>
    /// 获取或设置服务器IP地址
    /// </summary>
    public string? ServerIP { get; private set; }

    /// <summary>
    /// 获取或设置服务器端口号
    /// </summary>
    public int ServerPort { get; private set; }

    /// <summary>
    ///     是否正在运行udp组播订阅
    /// </summary>
    public bool IsRunning
    {
        get;
        set
        {
            if (value != field) this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    #endregion

    #region 公开接口方法

    private CancellationTokenSource? _connectServer;

    public void Start(string ip, int port)
    {
        ServerIP = ip;
        ServerPort = port;
        _connectServer = new CancellationTokenSource();
        Task.Run(async () =>
        {
            while (!_connectServer.IsCancellationRequested)
                try
                {
                    var ipAddress = IPAddress.Parse(ServerIP);
                    _udpIpEndPoint = new IPEndPoint(ipAddress, ServerPort);
                    _client = new UdpClient();
                    _client.JoinMulticastGroup(ipAddress);
                    IsRunning = true;

                    Logger.Info("Udp启动成功");
                    break;
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    Logger.Warn($"运行Udp异常，3秒后将重新运行：{ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
        }, _connectServer.Token);
    }

    public void Stop()
    {
        try
        {
            _connectServer?.Cancel();
            _client?.Close();
            _client = null;
            Logger.Info("停止Udp");
        }
        catch (Exception ex)
        {
            Logger.Warn($"停止Udp异常：{ex.Message}");
        }

        IsRunning = false;
    }

    public void SendCommand(INetObject command)
    {
        if (!IsRunning || _client == null) return;

        var buffer = command.Serialize(tcpHelper.SystemId);
        var sendCount = _client.Send(buffer, buffer.Length, _udpIpEndPoint);
        if (sendCount < buffer.Length)
        {
            Console.WriteLine($"UDP发送失败一包：{buffer.Length}=>{sendCount}");
        }
    }

    #endregion
}