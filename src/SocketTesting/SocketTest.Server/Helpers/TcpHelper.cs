using Avalonia.Threading;
using CodeWF.EventBus;
using ReactiveUI;
using SocketDto;
using SocketDto.Message;
using SocketNetObject;
using SocketNetObject.Models;
using SocketTest.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CodeWF.NetWeaver;
using CodeWF.NetWeaver.Base;

namespace SocketTest.Server.Helpers;

public class TcpHelper : ViewModelBase, ISocketBase
{
    private readonly ConcurrentDictionary<string, Socket> _clients = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<SocketMessage>> _requests = new();

    private Socket? _server;
    public long SystemId { get; } // 服务端标识，TCP数据接收时保存，用于UDP数据包识别

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

    private DateTime _heartbeatTime;

    /// <summary>
    ///     心跳时间
    /// </summary>
    public DateTime HeartbeatTime
    {
        get => _heartbeatTime;
        set => this.RaiseAndSetIfChanged(ref _heartbeatTime, value);
    }

    private int _mockCount = 200000;

    /// <summary>
    ///     模拟数据总量
    /// </summary>
    public int MockCount
    {
        get => _mockCount;
        set => this.RaiseAndSetIfChanged(ref _mockCount, value);
    }

    private int _mockPageSize = 5000;

    /// <summary>
    ///     模拟分包数据量
    /// </summary>
    public int MockPageSize
    {
        get => _mockPageSize;
        set => this.RaiseAndSetIfChanged(ref _mockPageSize, value);
    }

    #endregion

    #region 公开接口方法

    private CancellationTokenSource? _connectServer;

    public void Start()
    {
        _connectServer = new CancellationTokenSource();
        var ipEndPoint = new IPEndPoint(IPAddress.Parse(Ip!), Port);
        Task.Run(async () =>
        {
            while (!_connectServer.IsCancellationRequested)
                try
                {
                    _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _server.Bind(ipEndPoint);
                    _server.Listen(10);

                    await Dispatcher.UIThread.InvokeAsync(() => IsRunning = true);

                    ListenForClients();
                    ProcessingRequests();

                    Logger.Logger.Info($"Tcp服务启动成功：{ipEndPoint}，等待客户端连接");
                    Messenger.Default.Publish(this, new TcpStatusMessage(this, true));
                    break;
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    Logger.Logger.Warning($"运行TCP服务异常，3秒后将重新运行：{ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
        }, _connectServer.Token);
    }

    public void Stop()
    {
        try
        {
            _connectServer?.Cancel();
            _server?.Close(0);
            _server = null;
            Logger.Logger.Info("停止Tcp服务");
        }
        catch (Exception ex)
        {
            Logger.Logger.Warning($"停止TCP服务异常：{ex.Message}");
        }

        IsRunning = false;
    }

    public void SendCommand(INetObject command)
    {
        if (_clients.IsEmpty)
        {
            Logger.Logger.Error("没有客户端上线，无发送目的地址，无法发送命令");
            return;
        }

        var buffer = command.Serialize(SystemId);
        foreach (var client in _clients) client.Value.Send(buffer);

        Logger.Logger.Info($"发送命令{command.GetType()}");
    }

    public void SendCommand(Socket client, INetObject command)
    {
        var sw = Stopwatch.StartNew();
        var buffer = command.Serialize(SystemId);
        client.Send(buffer);

        if (command is not Heartbeat)
            Logger.Logger.Info($"发送命令{command.GetType()}，{buffer.Length}字节,{sw.ElapsedMilliseconds}ms");
    }

    private static int _taskId;

    public static int GetNewTaskId()
    {
        return ++_taskId;
    }

    #endregion

    #region 接收客户端命令

    private void RemoveClient(Socket tcpClient)
    {
        RemoveClient(tcpClient.RemoteEndPoint!.ToString()!);
    }

    private void RemoveClient(string key)
    {
        _clients.TryRemove(key, out _);
        _requests.TryRemove(key, out _);
        Logger.Logger.Warning($"已清除客户端信息{key}");
    }

    private void ListenForClients()
    {
        Task.Run(async () =>
        {
            while (IsRunning)
                try
                {
                    var socketClient = await _server!.AcceptAsync();

                    var socketClientKey = $"{socketClient.RemoteEndPoint}";
                    _clients[socketClientKey] = socketClient;

                    Logger.Logger.Info($"客户端({socketClientKey})连接上线");

                    HandleClient(socketClient);
                }
                catch (Exception ex)
                {
                    Logger.Logger.Error($"处理客户端连接上线异常：{ex.Message}");
                }
        });
    }

    private void HandleClient(Socket tcpClient)
    {
        Task.Run(() =>
        {
            while (IsRunning)
                try
                {
                    var tcpClientKey = tcpClient.RemoteEndPoint!.ToString()!;
                    while (tcpClient.ReadPacket(out var buffer, out var headInfo))
                    {
                        if (!_requests.TryGetValue(tcpClientKey, out var value))
                        {
                            value = new ConcurrentQueue<SocketMessage>();
                            _requests[tcpClientKey] = value;
                        }

                        value.Enqueue(new SocketMessage(this, headInfo!, buffer, tcpClient));
                    }
                }
                catch (SocketException ex)
                {
                    Logger.Logger.Error($"远程主机异常，将移除该客户端：{ex.Message}");
                    RemoveClient(tcpClient);
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Logger.Error($"接收数据异常：{ex.Message}");
                }

            return Task.CompletedTask;
        });
    }

    #endregion

    #region 处理客户端请求

    private void ProcessingRequests()
    {
        Task.Run(async () =>
        {
            while (IsRunning)
            {
                var needRemoveKeys = new List<string>();
                foreach (var request in _requests)
                {
                    var clientKey = request.Key;
                    if (!_clients.TryGetValue(clientKey, out var client))
                    {
                        needRemoveKeys.Add(clientKey);
                        continue;
                    }

                    while (request.Value.TryDequeue(out var command)) Messenger.Default.Publish(this, command);
                    ;
                }

                if (needRemoveKeys.Count > 0) needRemoveKeys.ForEach(RemoveClient);

                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        });
    }

    #endregion
}