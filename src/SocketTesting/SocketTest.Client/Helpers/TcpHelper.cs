﻿using Avalonia.Threading;
using CodeWF.EventBus;
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

public class TcpHelper : ReactiveObject, ISocketBase
{
    private Socket? _client;
    public long SystemId { get; private set; } // 服务端标识，TCP数据接收时保存，用于UDP数据包识别

    public readonly BlockingCollection<SocketCommand> _responses = new(new ConcurrentQueue<SocketCommand>());

    #region 公开属性

    private string? _ip = "127.0.0.1";

    /// <summary>
    ///     Tcp服务IP
    /// </summary>
    public string? Ip
    {
        get => _ip;
        set => this.RaiseAndSetIfChanged(ref _ip, value);
    }

    private int _port = 5000;

    /// <summary>
    ///     Tcp服务端口
    /// </summary>
    public int Port
    {
        get => _port;
        set => this.RaiseAndSetIfChanged(ref _port, value);
    }

    private bool _isRunning;

    /// <summary>
    ///     是否正在运行Tcp服务
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

    private DateTime _sendHeartbeatTime;

    /// <summary>
    ///     心跳发送时间
    /// </summary>
    public DateTime SendHeartbeatTime
    {
        get => _sendHeartbeatTime;
        set => this.RaiseAndSetIfChanged(ref _sendHeartbeatTime, value);
    }

    private DateTime _responseHeartbeatTime;

    /// <summary>
    ///     心跳响应时间
    /// </summary>
    public DateTime ResponseHeartbeatTime
    {
        get => _responseHeartbeatTime;
        set => this.RaiseAndSetIfChanged(ref _responseHeartbeatTime, value);
    }

    #endregion

    #region 公开接口

    private CancellationTokenSource? _connectServer;

    public void Start()
    {
        _connectServer = new CancellationTokenSource();
        var ipEndPoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
        Task.Run(async () =>
        {
            while (!_connectServer.IsCancellationRequested)
                try
                {
                    _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    await _client.ConnectAsync(ipEndPoint);

                    await Dispatcher.UIThread.InvokeAsync(() => IsRunning = true);

                    ListenForServer();
                    CheckResponse();

                    Logger.Info("连接Tcp服务成功");
                    await EventBus.Default.PublishAsync(new ChangeTCPStatusCommand(true, Ip, Port));
                    break;
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    Logger.Warn($"连接TCP服务异常，3秒后将重新连接：{ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
        }, _connectServer.Token);
    }

    public void Stop()
    {
        try
        {
            _connectServer?.Cancel();
            _client?.Close(0);
            Logger.Info("停止Tcp服务");
        }
        catch (Exception ex)
        {
            Logger.Warn($"停止TCP服务异常：{ex.Message}");
        }

        IsRunning = false;
    }

    public void SendCommand(INetObject command)
    {
        if (!IsRunning)
        {
            Logger.Error("Tcp服务未连接，无法发送命令");
            return;
        }

        var buffer = command.Serialize(SystemId);
        _client!.Send(buffer);
        var index = 0;
        buffer.ReadHead(ref index, out var head);
        Logger.Info($"Send(client={_client.RemoteEndPoint},len={buffer.Length})：{head}");
        if (command is Heartbeat)
        {
            SendHeartbeatTime = DateTime.Now;
        }
        else
            Logger.Info($"发送命令{command.GetType()}");
    }

    private static int _taskId;

    public static int GetNewTaskId()
    {
        return ++_taskId;
    }

    #endregion

    #region 连接TCP、接收数据

    private void ListenForServer()
    {
        Task.Run(() =>
        {
            while (IsRunning)
                try
                {
                    Logger.Info("Listen server");
                    while (_client!.ReadPacket(out var buffer, out var headInfo))
                    {
                        Logger.Info($"Receive(len={buffer.Length}): {headInfo}");
                        ReceiveTime = DateTime.Now;
                        SystemId = headInfo!.SystemId;
                        _responses.Add(new SocketCommand(headInfo, buffer, _client));
                    }
                }
                catch (SocketException ex)
                {
                    Logger.Error($"接收数据异常：{ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error($"接收数据异常：{ex.Message}");
                }

            return Task.CompletedTask;
        });
    }

    private void CheckResponse()
    {
        Task.Run(async () =>
        {
            while (!IsRunning)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }

            while (IsRunning)
            {
                while (_responses.TryTake(out var message, TimeSpan.FromMilliseconds(10)))
                {
                    Logger.Info($"Send event {message}");
                    await EventBus.Default.PublishAsync(message);
                }
            }
        });
    }

    #endregion
}