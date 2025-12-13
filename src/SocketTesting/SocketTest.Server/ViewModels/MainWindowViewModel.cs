using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CodeWF.EventBus;
using CodeWF.Log.Core;
using CodeWF.NetWeaver;
using CodeWF.Tools.Extensions;
using ReactiveUI;
using SocketDto;
using SocketDto.AutoCommand;
using SocketDto.Enums;
using SocketDto.EventBus;
using SocketDto.Requests;
using SocketDto.Response;
using SocketTest.Server.Helpers;
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

    public MainWindowViewModel()
    {
        void RegisterCommand()
        {
            var isTcpRunning = this.WhenAnyValue(x => x.TcpHelper.IsRunning);
            RefreshCommand = ReactiveCommand.CreateFromTask(HandleRefreshCommandAsync, isTcpRunning);
            UpdateCommand = ReactiveCommand.CreateFromTask(HandleUpdateCommandAsync, isTcpRunning);
        }

        TcpHelper = new TcpHelper();
        UdpHelper = new UdpHelper(TcpHelper);

        EventBus.Default.Subscribe(this);
        RegisterCommand();

        MockUpdate();
        MockSendData();

        Logger.Info("连接服务端后获取数据");
    }

    #region 属性

    public TcpHelper TcpHelper { get; set; }
    public UdpHelper UdpHelper { get; set; }

    /// <summary>
    ///     Tcp服务IP
    /// </summary>
    public string? TcpIp
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "127.0.0.1";

    /// <summary>
    ///     Tcp服务端口
    /// </summary>
    public int TcpPort
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 5000;

    /// <summary>
    ///     UDP组播IP
    /// </summary>
    public string? UdpIp
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "224.0.0.0";

    /// <summary>
    ///     UDP组播端口
    /// </summary>
    public int UdpPort
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 9540;

    public string? RunCommandContent
    {
        get => _runCommandContent;
        set => this.RaiseAndSetIfChanged(ref _runCommandContent, value);
    }


    /// <summary>
    ///     模拟数据总量
    /// </summary>
    public int MockCount
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 200000;

    /// <summary>
    ///     模拟分包数据量
    /// </summary>
    public int MockPageSize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 5000;

    /// <summary>
    ///     心跳时间
    /// </summary>
    public DateTime HeartbeatTime
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     是否正在运行Tcp服务
    /// </summary>
    public bool IsRunning
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     刷新数据
    /// </summary>
    public ReactiveCommand<Unit, Unit>? RefreshCommand { get; private set; }

    /// <summary>
    ///     更新数据
    /// </summary>
    public ReactiveCommand<Unit, Unit>? UpdateCommand { get; private set; }

    public async Task HandleRunCommandCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            TcpHelper.Start(TcpIp, TcpPort);
            UdpHelper.Start(UdpIp, UdpPort);
            IsRunning = true;
        }
        else
        {
            TcpHelper.Stop();
            UdpHelper.Stop();
            IsRunning = false;
        }

        await Task.CompletedTask;
    }

    #endregion 属性

    private async Task HandleRefreshCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            Logger.Error("未运行Tcp服务，无法发送命令");
            return;
        }

        UpdateAllData(true);
    }

    private async Task HandleUpdateCommandAsync()
    {
        if (!TcpHelper.IsRunning)
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
        if (message.IsMessage<RequestTargetType>())
        {
            ReceiveSocketMessage(message.Client!, message.Message<RequestTargetType>());
        }
        else if (message.IsMessage<RequestUdpAddress>())
        {
            ReceiveSocketMessage(message.Client!, message.Message<RequestUdpAddress>());
        }
        else if (message.IsMessage<RequestServiceInfo>())
        {
            ReceiveSocketMessage(message.Client!, message.Message<RequestServiceInfo>());
        }
        else if (message.IsMessage<RequestProcessIDList>())
        {
            ReceiveSocketMessage(message.Client!, message.Message<RequestProcessIDList>());
        }
        else if (message.IsMessage<RequestProcessList>())
        {
            ReceiveSocketMessage(message.Client!, message.Message<RequestProcessList>());
        }
        else if (message.IsMessage<ChangeProcessList>())
        {
            ReceiveSocketMessage(message.Client!, message.Message<ChangeProcessList>());
        }
        else if (message.IsMessage<Heartbeat>())
        {
            ReceiveSocketMessage(message.Client!, message.Message<Heartbeat>());
        }
    }

    private void ReceiveSocketMessage(Socket client, RequestTargetType request)
    {
        _ = Log("收到请求终端类型命令");
        var currentTerminalType = TerminalType.Server;

        var response = new ResponseTargetType()
        {
            TaskId = request.TaskId,
            Type = (byte)currentTerminalType
        };
        TcpHelper.SendCommand(client, response);

        _ = Log($"响应请求终端类型命令：当前终端为={currentTerminalType.GetDescription()}");
    }

    private void ReceiveSocketMessage(Socket client, RequestUdpAddress request)
    {
        _ = Log("收到请求Udp组播地址命令");

        var response = new ResponseUdpAddress()
        {
            TaskId = request.TaskId,
            Ip = UdpIp,
            Port = UdpPort,
        };
        TcpHelper.SendCommand(client, response);

        _ = Log($"响应请求Udp组播地址命令：{response.Ip}:{response.Port}");
    }

    private void ReceiveSocketMessage(Socket client, RequestServiceInfo request)
    {
        _ = Log("收到请求基本信息命令");

        var data = MockUtil.GetBaseInfoAsync().Result!;
        data.TaskId = request.TaskId;
        TcpHelper.SendCommand(client, data);

        _ = Log($"响应基本信息命令：当前操作系统版本号={data.OS}，内存大小={data.MemorySize}GB");
    }

    private void ReceiveSocketMessage(Socket client, RequestProcessIDList request)
    {
        _ = Log("收到请求进程ID列表命令");

        var response = new ResponseProcessIDList()
        {
            TaskId = request.TaskId,
            IDList = MockUtil.GetProcessIdListAsync().Result
        };
        TcpHelper.SendCommand(client, response);

        _ = Log($"响应进程ID列表命令：{response.IDList?.Length}");
    }

    private async Task ReceiveSocketMessage(Socket client, RequestProcessList request)
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
                TcpHelper.SendCommand(client, response);
                await Task.Delay(TimeSpan.FromMilliseconds(10));

                var msg = response.TaskId == default ? "推送" : "响应请求";
                Logger.Info(
                    $"{msg}【{response.PageIndex + 1}/{response.PageCount}】{response.Processes.Count}条({sendCount}/{response.TotalSize})");
            }
        });
    }

    private void ReceiveSocketMessage(Socket client, ChangeProcessList changeProcess)
    {
        TcpHelper.SendCommand(changeProcess);
    }

    private void ReceiveSocketMessage(Socket client, Heartbeat heartbeat)
    {
        TcpHelper.SendCommand(client, new Heartbeat());
        HeartbeatTime = DateTime.Now;
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
            TcpHelper.SendCommand(new ChangeProcessList());
            Logger.Info("====TCP推送结构变化通知====");
            return;
        }

        TcpHelper.SendCommand(new UpdateProcessList
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
        if (!UdpHelper.IsRunning || !MockUtil.IsInitOver) return;

        var sw = Stopwatch.StartNew();

        await MockUtil.MockUpdateDataAsync();

        sw.Stop();

        Logger.Info($"更新模拟实时数据{sw.ElapsedMilliseconds}ms");
    }

    private void MockSendRealtimeDataAsync(object? sender, ElapsedEventArgs e)
    {
        if (!UdpHelper.IsRunning || !MockUtil.IsInitOver) return;

        var sw = Stopwatch.StartNew();

        MockUtil.MockUpdateRealtimeProcessPageCount(MockCount, SerializeHelper.MaxUdpPacketSize, out var pageSize,
            out var pageCount);

        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            if (!UdpHelper.IsRunning) break;

            var response = MockUtil.MockUpdateRealtimeProcessList(pageSize, pageIndex);
            response.TotalSize = MockCount;
            response.PageSize = pageSize;
            response.PageCount = pageCount;
            response.PageIndex = pageIndex;

            UdpHelper.SendCommand(response);
        }

        Logger.Info(
            $"推送实时数据{MockCount}条，单包{pageSize}条分{pageCount}包，{sw.ElapsedMilliseconds}ms");
    }

    private void MockSendGeneralDataAsync(object? sender, ElapsedEventArgs e)
    {
        if (!UdpHelper.IsRunning || !MockUtil.IsInitOver) return;

        var sw = Stopwatch.StartNew();

        MockUtil.MockUpdateGeneralProcessPageCount(MockCount, SerializeHelper.MaxUdpPacketSize, out var pageSize,
            out var pageCount);

        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            if (!UdpHelper.IsRunning) break;

            var response = MockUtil.MockUpdateGeneralProcessList(pageSize, pageIndex);
            response.TotalSize = MockCount;
            response.PageSize = pageSize;
            response.PageCount = pageCount;
            response.PageIndex = pageIndex;

            UdpHelper.SendCommand(response);
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