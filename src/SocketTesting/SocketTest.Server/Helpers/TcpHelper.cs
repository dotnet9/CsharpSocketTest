using ReactiveUI;
using SocketDto;
using SocketNetObject;
using SocketNetObject.Models;
using SocketTest.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Threading;
using SocketTest.Server.Mock;

namespace SocketTest.Server.Helpers;

public class TcpHelper : ViewModelBase, ISocketBase
{
    private readonly ConcurrentDictionary<string, Socket> _clients = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<INetObject>> _requests = new();

    private Socket? _server;
    public long SystemId { get; } // 服务端标识，TCP数据接收时保存，用于UDP数据包识别

    #region 公开属性

    private string _ip = "127.0.0.1";

    /// <summary>
    ///     Tcp服务IP
    /// </summary>
    public string Ip
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

    private bool _isStarted;

    /// <summary>
    ///     是否开启Tcp服务
    /// </summary>
    public bool IsStarted
    {
        get => _isStarted;
        set
        {
            if (value != _isStarted) this.RaiseAndSetIfChanged(ref _isStarted, value);
        }
    }

    private bool _isRunning;

    /// <summary>
    ///     是否正在运行Tcp服务
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

    private DateTime _heartbeatTime;

    /// <summary>
    /// 心跳时间
    /// </summary>
    public DateTime HeartbeatTime
    {
        get => _heartbeatTime;
        set => this.RaiseAndSetIfChanged(ref _heartbeatTime, value);
    }

    /// <summary>
    /// 已发送UDP包个数
    /// </summary>
    public int UDPPacketsSentCount { get; set; } = 0;

    private int _mockCount = 1000000;

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

    public void Start()
    {
        if (IsStarted)
        {
            Logger.Logger.Warning("Tcp服务已经开启");
            return;
        }

        IsStarted = true;

        var ipEndPoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
        Task.Run(async () =>
        {
            while (IsStarted)
                try
                {
                    _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _server.Bind(ipEndPoint);
                    _server.Listen(10);

                    Dispatcher.UIThread.InvokeAsync(() => IsRunning = true);

                    ListenForClients();
                    ProcessingRequests();
                    MockUpdate();

                    Logger.Logger.Info($"Tcp服务启动成功：{ipEndPoint}，等待客户端连接");
                    break;
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    Logger.Logger.Warning($"运行TCP服务异常，3秒后将重新运行：{ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
        });
    }

    public void Stop()
    {
        if (!IsStarted)
        {
            Logger.Logger.Warning("Tcp服务已经关闭");
            return;
        }

        IsStarted = false;

        try
        {
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
        foreach (var client in _clients)
        {
            client.Value.Send(buffer);
        }

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

    public bool TryGetResponse(out INetObject? response)
    {
        throw new NotImplementedException();
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
                    while (tcpClient.ReadPacket(out var buffer, out var objectInfo))
                        ReadCommand(tcpClient, buffer, objectInfo!);
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

    private void ReadCommand(Socket tcpClient, byte[] buffer, NetHeadInfo netObjectHeadInfo)
    {
        INetObject command;

        if (netObjectHeadInfo.IsNetObject<RequestBaseInfo>())
            command = buffer.Deserialize<RequestBaseInfo>();
        else if (netObjectHeadInfo.IsNetObject<RequestProcessList>())
            command = buffer.Deserialize<RequestProcessList>();
        else if (netObjectHeadInfo.IsNetObject<ChangeProcessList>())
            command = buffer.Deserialize<ChangeProcessList>();
        else if (netObjectHeadInfo.IsNetObject<Heartbeat>())
            command = buffer.Deserialize<Heartbeat>();
        else
            throw new Exception(
                $"非法数据包：{netObjectHeadInfo}");

        var tcpClientKey = tcpClient.RemoteEndPoint!.ToString()!;
        if (!_requests.TryGetValue(tcpClientKey, out var value))
        {
            value = new ConcurrentQueue<INetObject>();
            _requests[tcpClientKey] = value;
        }

        value.Enqueue(command);
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

                    while (request.Value.TryDequeue(out var command)) ProcessingRequest(client, command);
                }

                if (needRemoveKeys.Count > 0) needRemoveKeys.ForEach(RemoveClient);

                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        });
    }

    private void ProcessingRequest(Socket tcpClient, INetObject request)
    {
        switch (request)
        {
            case RequestBaseInfo requestBaseInfo:
                ProcessingRequest(tcpClient, requestBaseInfo);
                break;
            case RequestProcessList requestProcess:
                ProcessingRequest(tcpClient, requestProcess);
                break;
            case ChangeProcessList changeProcess:
                ProcessingRequest(tcpClient, changeProcess);
                break;
            case Heartbeat _:
                ProcessingRequest(tcpClient);
                break;
            default:
                throw new Exception($"未处理命令{request.GetType().Name}");
        }
    }

    private void ProcessingRequest(Socket client, RequestBaseInfo request)
    {
        SendCommand(client, MockUtil.MockBase(request.TaskId));
    }

    private async void ProcessingRequest(Socket client, RequestProcessList request)
    {
        var pageCount = MockUtil.GetPageCount(MockCount, MockPageSize);
        var sendCount = 0;
        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            var response = new ResponseProcessList
            {
                TaskId = request.TaskId,
                TotalSize = MockCount,
                PageSize = MockPageSize,
                PageCount = pageCount,
                PageIndex = pageIndex,
                Processes = await MockUtil.MockProcessesAsync(MockCount, MockPageSize, pageIndex)
            };
            sendCount += response.Processes.Count;
            SendCommand(client, response);
            await Task.Delay(TimeSpan.FromMilliseconds(10));

            var msg = response.TaskId == default ? "推送" : "响应请求";
            Logger.Logger.Info(
                $"{msg}【{response.PageIndex + 1}/{response.PageCount}】{response.Processes.Count}条({sendCount}/{response.TotalSize})");
        }
    }

    private void ProcessingRequest(Socket client, ChangeProcessList changeProcess)
    {
        SendCommand(changeProcess);
    }

    private void ProcessingRequest(Socket client)
    {
        SendCommand(client, new Heartbeat { UDPPacketsSentCount = UDPPacketsSentCount });
        HeartbeatTime = DateTime.Now;
    }

    #endregion

    #region 更新数据

    private System.Timers.Timer? _sendDataTimer;
    private bool _isUpdateAll;

    public void UpdateAllData(bool isUpdateAll)
    {
        _isUpdateAll = isUpdateAll;
        MockSendData(null, null);
    }

    private void MockUpdate()
    {
        _sendDataTimer = new System.Timers.Timer();
        _sendDataTimer.Interval = 4 * 60 * 1000;
        _sendDataTimer.Elapsed += MockSendData;
        _sendDataTimer.Start();
    }

    private async void MockSendData(object? sender, ElapsedEventArgs? e)
    {
        if (_isUpdateAll)
        {
            SendCommand(new ChangeProcessList());
            Logger.Logger.Info("====TCP推送结构变化通知====");
            return;
        }

        SendCommand(new UpdateProcessList
        {
            Processes = await MockUtil.MockProcessesAsync(MockCount, MockPageSize)
        });
        Logger.Logger.Info("====TCP推送更新通知====");

        _isUpdateAll = !_isUpdateAll;
    }

    #endregion
}