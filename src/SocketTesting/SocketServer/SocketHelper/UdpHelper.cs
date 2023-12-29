﻿using System.Timers;
using Timer = System.Timers.Timer;

namespace SocketServer.SocketHelper;

public class UdpHelper(TcpHelper tcpHelper) : BindableBase, ISocketBase
{
    private UdpClient? _client;
    private IPEndPoint? _udpIpEndPoint;

    #region 公开属性

    private string _ip = "224.0.0.0";

    /// <summary>
    ///     UDP组播IP
    /// </summary>
    public string Ip
    {
        get => _ip;
        set => SetProperty(ref _ip, value);
    }

    private int _port = 9540;

    /// <summary>
    ///     UDP组播端口
    /// </summary>
    public int Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    private bool _isStarted;

    public bool IsStarted
    {
        get => _isStarted;
        set
        {
            if (value != _isStarted) SetProperty(ref _isStarted, value);
        }
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
            if (value != _isRunning) SetProperty(ref _isRunning, value);
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
            if (value != _sendTime) SetProperty(ref _sendTime, value);
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
            if (value != _receiveTime) SetProperty(ref _receiveTime, value);
        }
    }

    private int _packetMaxSize = 65507;

    /// <summary>
    ///     Udp单包大小上限
    /// </summary>
    public int PacketMaxSize
    {
        get => _packetMaxSize;
        set => SetProperty(ref _packetMaxSize, value);
    }

    #endregion

    #region 公开接口方法

    public void Start()
    {
        if (IsStarted)
        {
            Logger.Warning("Udp组播已经开启");
            return;
        }

        IsStarted = true;

        Task.Run(() =>
        {
            while (IsStarted)
                try
                {
                    var ipAddress = IPAddress.Parse(Ip);
                    _udpIpEndPoint = new IPEndPoint(ipAddress, Port);
                    _client = new UdpClient();
                    _client.JoinMulticastGroup(ipAddress);
                    IsRunning = true;

                    MockSendData();

                    Logger.Info("Udp启动成功");
                    break;
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    Logger.Warning($"运行Udp异常，3秒后将重新运行：{ex.Message}");
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                }
        });
    }

    public void Stop()
    {
        if (!IsStarted)
        {
            Logger.Warning("Udp组播已经关闭");
            return;
        }

        IsStarted = false;

        try
        {
            _client?.Close();
            _client = null;
            Logger.Info("停止Udp");
        }
        catch (Exception ex)
        {
            Logger.Warning($"停止Udp异常：{ex.Message}");
        }

        IsRunning = false;
    }

    public void SendCommand(INetObject command)
    {
    }

    public bool TryGetResponse(out INetObject? response)
    {
        response = null;
        return false;
    }

    #endregion

    #region 模拟数据更新

    private Timer _updateDataTimer;
    private Timer _sendDataTimer;

    private void MockSendData()
    {
        _updateDataTimer = new Timer();
        _updateDataTimer.Interval = 500;
        _updateDataTimer.Elapsed += MockUpdateData;
        _updateDataTimer.Start();

        _sendDataTimer = new Timer();
        _sendDataTimer.Interval = MockUtil.UdpSendMilliseconds;
        _sendDataTimer.Elapsed += MockSendData;
        _sendDataTimer.Start();
    }

    private void MockUpdateData(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (!IsRunning) return;

        var sw = Stopwatch.StartNew();

        MockUtil.MockUpdateProcess(tcpHelper.MockCount);
        sw.Stop();

        Logger.Info($"更新模拟实时数据{sw.ElapsedMilliseconds}ms");
    }

    private void MockSendData(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (!IsRunning) return;

        var sw = Stopwatch.StartNew();

        MockUtil.MockUpdateActiveProcessPageCount(tcpHelper.MockCount, PacketMaxSize, out var pageSize,
            out var pageCount);

        var size = 0;
        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            if (!IsRunning) break;

            var response = new UpdateActiveProcessList
            {
                TotalSize = tcpHelper.MockCount,
                PageSize = pageSize,
                PageCount = pageCount,
                PageIndex = pageIndex,
                Processes = MockUtil.MockUpdateProcess(tcpHelper.MockCount, pageSize, pageIndex)
            };

            var buffer = response.SerializeByNative(tcpHelper.SystemId);
            tcpHelper.UDPPacketsSentCount++;
            size += _client!.Send(buffer, buffer.Length, _udpIpEndPoint);
        }

        Logger.Info($"推送实时数据{tcpHelper.MockCount}条，单包{pageSize}条分{pageCount}包，成功发送{size}字节，{sw.ElapsedMilliseconds}ms");
    }

    #endregion
}