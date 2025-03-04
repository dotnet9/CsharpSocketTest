﻿using CodeWF.EventBus;
using CodeWF.LogViewer.Avalonia;
using CodeWF.NetWeaver;
using CodeWF.NetWeaver.Base;
using ReactiveUI;
using SocketDto;
using SocketDto.EventBus;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest.Client.Helpers;

public class UdpHelper : ReactiveObject, ISocketBase
{
    private readonly BlockingCollection<SocketCommand> _receivedBuffers = new(new ConcurrentQueue<SocketCommand>());
    private UdpClient? _client;
    private IPEndPoint _remoteEp = new(IPAddress.Any, 0);

    #region 公开属性

    private string? _ip;

    /// <summary>
    ///     UDP组播IP
    /// </summary>
    public string? Ip
    {
        get => _ip;
        set => this.RaiseAndSetIfChanged(ref _ip, value);
    }

    private int _port;

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
        set => this.RaiseAndSetIfChanged(ref _isRunning, value);
    }

    private DateTime _sendTime;

    /// <summary>
    ///     命令发送时间
    /// </summary>
    public DateTime SendTime
    {
        get => _sendTime;
        set => this.RaiseAndSetIfChanged(ref _sendTime, value);
    }

    private DateTime _receiveTime;

    /// <summary>
    ///     响应接收时间
    /// </summary>
    public DateTime ReceiveTime
    {
        get => _receiveTime;
        set => this.RaiseAndSetIfChanged(ref _receiveTime, value);
    }

    /// <summary>
    /// 新数据通知
    /// </summary>
    public Action<SocketCommand>? NewDataResponse;

    #endregion

    #region 公开接口

    private CancellationTokenSource? _connectServer;

    public void Start()
    {
        _connectServer = new CancellationTokenSource();
        Task.Run(async () =>
        {
            while (!_connectServer.IsCancellationRequested)
                try
                {
                    _client = new UdpClient();
                    _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                    _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                    // 任意IP+广播端口，0是任意端口
                    _client.Client.Bind(new IPEndPoint(IPAddress.Any, Port));

                    // 加入组播
                    _client.JoinMulticastGroup(IPAddress.Parse(Ip!));
                    IsRunning = true;

                    ReceiveData();
                    CheckMessage();
                    await EventBus.Default.PublishAsync(new ChangeUDPStatusCommand(true));
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
    }

    #endregion

    #region 接收处理数据

    private void ReceiveData()
    {
        Task.Run(async () =>
        {
            while (IsRunning)
                try
                {
                    if (_client?.Client == null || _client.Available < 0)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(10));
                        continue;
                    }

                    var data = _client.Receive(ref _remoteEp);
                    var readIndex = 0;
                    if (SerializeHelper.ReadHead(data, ref readIndex, out var headInfo) &&
                        data.Length >= headInfo?.BufferLen)
                    {
                        _receivedBuffers.Add(new SocketCommand(headInfo!, data));
                    }
                    else
                    {
                        Logger.Warn($"收到错误UDP包：{headInfo}");
                    }

                    ReceiveTime = DateTime.Now;
                }
                catch (SocketException ex)
                {
                    Logger.Error(ex.SocketErrorCode == SocketError.Interrupted
                        ? "Udp中断，停止接收数据！"
                        : $"接收Udp数据异常：{ex.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"接收Udp数据异常：{ex.Message}");
                }
        });
    }

    private void CheckMessage()
    {
        Task.Run(async () =>
        {
            while (!IsRunning)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }

            while (IsRunning)
            {
                while (_receivedBuffers.TryTake(out var message, TimeSpan.FromMilliseconds(10)))
                {
                    NewDataResponse?.Invoke(message);
                }
            }
        });
    }

    #endregion
}