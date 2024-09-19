using CodeWF.NetWeaver;
using CodeWF.NetWeaver.Base;
using ReactiveUI;
using SocketNetObject.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CodeWF.LogViewer.Avalonia;

namespace SocketTest.Server.Helpers;

public class UdpHelper(TcpHelper tcpHelper) : ReactiveObject, ISocketBase
{
    private UdpClient? _client;
    private IPEndPoint? _udpIpEndPoint;

    #region 公开属性

    private string? _ip = "224.0.0.0";

    /// <summary>
    ///     UDP组播IP
    /// </summary>
    public string? Ip
    {
        get => _ip;
        set => this.RaiseAndSetIfChanged(ref _ip, value);
    }

    private int _port = 9540;

    /// <summary>
    ///     UDP组播端口
    /// </summary>
    public int Port
    {
        get => _port;
        set => this.RaiseAndSetIfChanged(ref _port, value);
    }

    private bool _isRunning;

    /// <summary>
    ///     是否正在运行udp组播订阅
    /// </summary>
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (value != _isRunning) this.RaiseAndSetIfChanged(ref _isRunning, value);
        }
    }

    private DateTime _sendTime;

    /// <summary>
    ///     命令发送时间
    /// </summary>
    public DateTime SendTime
    {
        get => _sendTime;
        set
        {
            if (value != _sendTime) this.RaiseAndSetIfChanged(ref _sendTime, value);
        }
    }

    private DateTime _receiveTime;

    /// <summary>
    ///     响应接收时间
    /// </summary>
    public DateTime ReceiveTime
    {
        get => _receiveTime;
        set
        {
            if (value != _receiveTime) this.RaiseAndSetIfChanged(ref _receiveTime, value);
        }
    }

    private int _packetMaxSize = 65507;

    /// <summary>
    ///     Udp单包大小上限
    /// </summary>
    public int PacketMaxSize
    {
        get => _packetMaxSize;
        set => this.RaiseAndSetIfChanged(ref _packetMaxSize, value);
    }

    #endregion

    #region 公开接口方法

    private CancellationTokenSource? _connectServer;

    public void Start()
    {
        _connectServer = new CancellationTokenSource();
        Task.Run(async () =>
        {
            while (!_connectServer.IsCancellationRequested)
                try
                {
                    var ipAddress = IPAddress.Parse(Ip);
                    _udpIpEndPoint = new IPEndPoint(ipAddress, Port);
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