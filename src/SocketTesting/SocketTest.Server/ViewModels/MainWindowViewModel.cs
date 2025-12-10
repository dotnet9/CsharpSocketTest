using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CodeWF.EventBus;
using CodeWF.Log.Core;
using CodeWF.NetWeaver;
using CodeWF.NetWrapper.Commands;
using CodeWF.NetWrapper.Helpers;
using CodeWF.NetWrapper.Models;
using CodeWF.Tools.Extensions;
using ReactiveUI;
using SocketDto;
using SocketDto.AutoCommand;
using SocketDto.Enums;
using SocketDto.EventBus;
using SocketDto.Requests;
using SocketDto.Response;
using SocketTest.Server.Mock;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reactive;
using System.Threading.Tasks;
using System.Timers;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace SocketTest.Server.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    public WindowNotificationManager? NotificationManager { get; set; }
    private string? _runCommandContent = "开启服务";
    private TcpSocketServer _tcpServer { get; set; }
    private UdpSocketServer _udpServer { get; set; }

    public MainWindowViewModel()
    {
        void RegisterCommand()
        {
            var isTcpRunning = this.WhenAnyValue(x => x._tcpServer.IsRunning);
            isTcpRunning.Subscribe(running => IsRunning = running);
            RefreshCommand = ReactiveCommand.CreateFromTask(HandleRefreshCommandAsync, isTcpRunning);
            UpdateCommand = ReactiveCommand.CreateFromTask(HandleUpdateCommandAsync, isTcpRunning);
        }

        _tcpServer = new TcpSocketServer();
        _udpServer = new UdpSocketServer();

        EventBus.Default.Subscribe(this);
        RegisterCommand();

        MockUpdate();
        MockSendData();

        Logger.Info("连接服务端后获取数据");
    }

    #region 属性
    public string? TCPIP
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "127.0.0.1";

    public int TCPPort
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 5000;
    public string? UDPIP
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "224.0.0.0";

    public int UDPPort
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 9540;

    public bool IsRunning
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
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

    public string? RunCommandContent
    {
        get => _runCommandContent;
        set => this.RaiseAndSetIfChanged(ref _runCommandContent, value);
    }

    /// <summary>
    ///     刷新数据
    /// </summary>
    public ReactiveCommand<Unit, Unit>? RefreshCommand { get; private set; }

    /// <summary>
    ///     更新数据
    /// </summary>
    public ReactiveCommand<Unit, Unit>? UpdateCommand { get; private set; }

    #endregion 属性

    public async Task HandleRunCommandCommandAsync()
    {
        if (!_tcpServer.IsRunning)
        {
            _tcpServer.StartAsync("TCP服务端", TCPIP, TCPPort);
            //_udpServer.Start("UDP服务端", UDPIP, UDPPort, "0.0.0.0");
        }
        else
        {
            await _tcpServer.StopAsync();
            _udpServer.Stop();
        }

        await Task.CompletedTask;
    }

    private async Task HandleRefreshCommandAsync()
    {
        if (!_tcpServer.IsRunning)
        {
            Logger.Error("未运行Tcp服务，无法发送命令");
            return;
        }

        UpdateAllData(true);
    }

    private async Task HandleUpdateCommandAsync()
    {
        if (!_tcpServer.IsRunning)
        {
            Logger.Error("未运行Tcp服务，无法发送命令");
            return;
        }

        UpdateAllData(false);
    }

    #region 处理Socket信息

    [EventHandler]
    private async Task ReceiveTcpStatusMessageAsync(ChangeTCPStatusCommand message)
    {
        _ = Log(message.IsConnect ? "TCP服务已运行" : "TCP服务已停止");
        if (message.IsConnect)
        {
            Task.Run(async () =>
            {
                await MockUtil.MockAsync(MockCount);
                _ = Log("数据模拟完成，客户端可以正常请求数据了");
            });
        }
    }

    [EventHandler]
    private async Task ReceiveSocketMessageAsync(SocketCommand message)
    {
        Logger.Info($"Dill command: {message}");
        if (message.IsCommand<RequestTargetType>())
        {
            await ReceiveSocketMessageAsync(message.Client!, message.GetCommand<RequestTargetType>());
        }
        else if (message.IsCommand<RequestUdpAddress>())
        {
            await ReceiveSocketMessageAsync(message.Client!, message.GetCommand<RequestUdpAddress>());
        }
        else if (message.IsCommand<RequestServiceInfo>())
        {
            await ReceiveSocketMessageAsync(message.Client!, message.GetCommand<RequestServiceInfo>());
        }
        else if (message.IsCommand<RequestProcessIDList>())
        {
            await ReceiveSocketMessageAsync(message.Client!, message.GetCommand<RequestProcessIDList>());
        }
        else if (message.IsCommand<RequestProcessList>())
        {
            await ReceiveSocketMessageAsync(message.Client!, message.GetCommand<RequestProcessList>());
        }
        else if (message.IsCommand<ChangeProcessList>())
        {
            await ReceiveSocketMessageAsync(message.Client!, message.GetCommand<ChangeProcessList>());
        }
        else if (message.IsCommand<Heartbeat>())
        {
            await ReceiveSocketMessageAsync(message.Client!, message.GetCommand<Heartbeat>());
        }
    }

    private async Task ReceiveSocketMessageAsync(Socket client, RequestTargetType request)
    {
        _ = Log("收到请求终端类型命令");
        var currentTerminalType = TerminalType.Server;

        var response = new ResponseTargetType()
        {
            TaskId = request.TaskId,
            Type = (byte)currentTerminalType
        };
        await _tcpServer.SendCommandAsync(client, response);

        _ = Log($"响应请求终端类型命令：当前终端为={currentTerminalType.GetDescription()}");
    }

    private async Task ReceiveSocketMessageAsync(Socket client, RequestUdpAddress request)
    {
        _ = Log("收到请求Udp组播地址命令");

        var response = new ResponseUdpAddress()
        {
            TaskId = request.TaskId,
            Ip = _udpServer.ServerIP,
            Port = _udpServer.ServerPort,
        };
        await _tcpServer.SendCommandAsync(client, response);

        _ = Log($"响应请求Udp组播地址命令：{response.Ip}:{response.Port}");
    }

    private async Task ReceiveSocketMessageAsync(Socket client, RequestServiceInfo request)
    {
        _ = Log("收到请求基本信息命令");

        var data = MockUtil.GetBaseInfoAsync().Result!;
        data.TaskId = request.TaskId;
        await _tcpServer.SendCommandAsync(client, data);

        _ = Log($"响应基本信息命令：当前操作系统版本号={data.OS}，内存大小={data.MemorySize}GB");
    }

    private async Task ReceiveSocketMessageAsync(Socket client, RequestProcessIDList request)
    {
        _ = Log("收到请求进程ID列表命令");

        var response = new ResponseProcessIDList()
        {
            TaskId = request.TaskId,
            IDList = MockUtil.GetProcessIdListAsync().Result
        };
        await _tcpServer.SendCommandAsync(client, response);

        _ = Log($"响应进程ID列表命令：{response.IDList?.Length}");
    }

    private async Task ReceiveSocketMessageAsync(Socket client, RequestProcessList request)
    {
        _ = Log("收到请求进程详细信息列表命令");
        await Task.Run(async () =>
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
                    Processes = await MockUtil.MockProcessesAsync(MockPageSize, pageIndex)
                };
                sendCount += response.Processes.Count;
                await _tcpServer.SendCommandAsync(client, response);
                await Task.Delay(TimeSpan.FromMilliseconds(10));

                var msg = response.TaskId == default ? "推送" : "响应请求";
                Logger.Info(
                    $"{msg}【{response.PageIndex + 1}/{response.PageCount}】{response.Processes.Count}条({sendCount}/{response.TotalSize})");
            }
        });
    }

    private async Task ReceiveSocketMessageAsync(Socket client, ChangeProcessList changeProcess)
    {
        await _tcpServer.SendCommandAsync(changeProcess);
    }

    private async Task ReceiveSocketMessageAsync(Socket client, Heartbeat heartbeat)
    {
        await _tcpServer.SendCommandAsync(client, new Heartbeat());
        _tcpServer.HeartbeatTime = DateTime.Now;
    }

    #endregion


    #region 更新数据

    private Timer? _sendDataTimer;
    private bool _isUpdateAll;

    public void UpdateAllData(bool isUpdateAll)
    {
        _isUpdateAll = isUpdateAll;
        MockSendData(null, null);
    }

    private void MockUpdate()
    {
        _sendDataTimer = new Timer();
        _sendDataTimer.Interval = 4 * 60 * 1000;
        _sendDataTimer.Elapsed += MockSendData;
        _sendDataTimer.Start();
    }

    private async void MockSendData(object? sender, ElapsedEventArgs? e)
    {
        if (_isUpdateAll)
        {
            _tcpServer.SendCommandAsync(new ChangeProcessList());
            Logger.Info("====TCP推送结构变化通知====");
            return;
        }

        _tcpServer.SendCommandAsync(new UpdateProcessList
        {
            Processes = await MockUtil.MockProcessesAsync(MockCount, MockPageSize)
        });
        Logger.Info("====TCP推送更新通知====");

        _isUpdateAll = !_isUpdateAll;
    }

    #endregion


    #region 模拟数据更新

    private Timer? _updateDataTimer;
    private Timer? _sendRealtimeDataTimer;
    private Timer? _sendGeneralDataTimer;

    private void MockSendData()
    {
        _updateDataTimer = new Timer();
        _updateDataTimer.Interval = MockConst.UdpUpdateMilliseconds;
        _updateDataTimer.Elapsed += MockUpdateDataAsync;
        _updateDataTimer.Start();

        _sendRealtimeDataTimer = new Timer();
        _sendRealtimeDataTimer.Interval = MockConst.UdpSendRealtimeMilliseconds;
        _sendRealtimeDataTimer.Elapsed += MockSendRealtimeDataAsync;
        _sendRealtimeDataTimer.Start();

        _sendGeneralDataTimer = new Timer();
        _sendGeneralDataTimer.Interval = MockConst.UdpSendGeneralMilliseconds;
        _sendGeneralDataTimer.Elapsed += MockSendGeneralDataAsync;
        _sendGeneralDataTimer.Start();
    }

    private async void MockUpdateDataAsync(object? sender, ElapsedEventArgs e)
    {
        if (!_udpServer.IsRunning || !MockUtil.IsInitOver) return;

        var sw = Stopwatch.StartNew();

        await MockUtil.MockUpdateDataAsync();

        sw.Stop();

        Logger.Info($"更新模拟实时数据{sw.ElapsedMilliseconds}ms");
    }

    private void MockSendRealtimeDataAsync(object? sender, ElapsedEventArgs e)
    {
        if (!_udpServer.IsRunning || !MockUtil.IsInitOver) return;

        var sw = Stopwatch.StartNew();

        MockUtil.MockUpdateRealtimeProcessPageCount(MockCount, SerializeHelper.MaxUdpPacketSize, out var pageSize,
            out var pageCount);

        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            if (!_udpServer.IsRunning) break;

            var response = MockUtil.MockUpdateRealtimeProcessList(pageSize, pageIndex);
            response.TotalSize = MockCount;
            response.PageSize = pageSize;
            response.PageCount = pageCount;
            response.PageIndex = pageIndex;

            _udpServer.SendCommand(response, DateTimeOffset.UtcNow);
        }

        Logger.Info(
            $"推送实时数据{MockCount}条，单包{pageSize}条分{pageCount}包，{sw.ElapsedMilliseconds}ms");
    }

    private void MockSendGeneralDataAsync(object? sender, ElapsedEventArgs e)
    {
        if (!_udpServer.IsRunning || !MockUtil.IsInitOver) return;

        var sw = Stopwatch.StartNew();

        MockUtil.MockUpdateGeneralProcessPageCount(MockCount, SerializeHelper.MaxUdpPacketSize, out var pageSize,
            out var pageCount);

        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            if (!_udpServer.IsRunning) break;

            var response = MockUtil.MockUpdateGeneralProcessList(pageSize, pageIndex);
            response.TotalSize = MockCount;
            response.PageSize = pageSize;
            response.PageCount = pageCount;
            response.PageIndex = pageIndex;

            _udpServer.SendCommand(response, DateTimeOffset.UtcNow);
        }

        Logger.Info(
            $"推送一般数据{MockCount}条，单包{pageSize}条分{pageCount}包，{sw.ElapsedMilliseconds}ms");
    }

    #endregion

    private void Invoke(Action action)
    {
        Dispatcher.UIThread.Post(action.Invoke);
    }

    private async Task InvokeAsync(Action action)
    {
        await Dispatcher.UIThread.InvokeAsync(action.Invoke);
    }

    private async Task Log(string msg, LogType type = LogType.Info, bool showNotification = true)
    {
        if (type == LogType.Info)
        {
            Logger.Info(msg);
        }
        else if (type == LogType.Error)
        {
            Logger.Error(msg);
        }

        await ShowNotificationAsync(showNotification, msg, type);
    }

    private async Task ShowNotificationAsync(bool showNotification, string msg, LogType type)
    {
        if (!showNotification) return;

        var notificationType = type switch
        {
            LogType.Warn => NotificationType.Warning,
            LogType.Error => NotificationType.Error,
            _ => NotificationType.Information
        };

        await InvokeAsync(() => NotificationManager?.Show(new Notification(title: "提示", msg, notificationType)));
    }
}