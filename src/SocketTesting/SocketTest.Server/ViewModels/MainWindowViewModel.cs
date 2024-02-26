using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Messager;
using ReactiveUI;
using SocketDto;
using SocketDto.AutoCommand;
using SocketDto.Enums;
using SocketDto.Message;
using SocketDto.Requests;
using SocketDto.Response;
using SocketTest.Common;
using SocketTest.Logger.Models;
using SocketTest.Mvvm;
using SocketTest.Server.Helpers;
using SocketTest.Server.Mock;
using System;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Timers;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace SocketTest.Server.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public WindowNotificationManager? NotificationManager { get; set; }
    private string? _runCommandContent = "开启服务";

    public MainWindowViewModel()
    {
        void ListenProperty()
        {
            this.WhenAnyValue(x => x.TcpHelper.IsRunning)
                .Subscribe(newValue => RunCommandContent = newValue ? "停止服务" : "开启服务");
        }

        void RegisterEvent()
        {
            Messenger.Default.Subscribe<SocketMessage>(this, ReceiveSocketMessage);
            Messenger.Default.Subscribe<TcpStatusMessage>(this, ReceiveTcpStatusMessage);
        }

        void RegisterCommand()
        {
            var isTcpRunning = this.WhenAnyValue(x => x.TcpHelper.IsRunning);
            RefreshCommand = ReactiveCommand.Create(HandleRefreshCommandAsync, isTcpRunning);
            UpdateCommand = ReactiveCommand.Create(HandleUpdateCommandAsync, isTcpRunning);
        }

        TcpHelper = new TcpHelper();
        UdpHelper = new UdpHelper(TcpHelper);

        ListenProperty();
        RegisterEvent();
        RegisterCommand();

        MockUpdate();
        MockSendData();

        Logger.Logger.Info("连接服务端后获取数据");
    }

    public TcpHelper TcpHelper { get; set; }
    public UdpHelper UdpHelper { get; set; }

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

    public async Task HandleRunCommandCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            TcpHelper.Start();
            UdpHelper.Start();
        }
        else
        {
            TcpHelper.Stop();
            UdpHelper.Stop();
        }

        await Task.CompletedTask;
    }

    private void HandleRefreshCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            Logger.Logger.Error("未运行Tcp服务，无法发送命令");
            return;
        }

        UpdateAllData(true);
    }

    private void HandleUpdateCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            Logger.Logger.Error("未运行Tcp服务，无法发送命令");
            return;
        }

        UpdateAllData(false);
    }

    #region 处理Socket信息

    private void ReceiveTcpStatusMessage(TcpStatusMessage message)
    {
        _ = Log(message.IsConnect ? "TCP服务已运行" : "TCP服务已停止");
        if (message.IsConnect)
        {
            Task.Run(async () =>
            {
                await MockUtil.MockAsync(TcpHelper.MockCount);
                _ = Log("数据模拟完成，客户端可以正常请求数据了");
            });
        }
    }

    private void ReceiveSocketMessage(SocketMessage message)
    {
        if (message.IsMessage<RequestTargetType>())
        {
            ReceiveSocketMessage(message.Client!, message.Message<RequestTargetType>());
        }

        if (message.IsMessage<RequestUdpAddress>())
        {
            ReceiveSocketMessage(message.Client!, message.Message<RequestUdpAddress>());
        }
        else if (message.IsMessage<RequestServiceInfo>())
        {
            ReceiveSocketMessage(message.Client!, message.Message<RequestServiceInfo>());
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

        _ = Log($"响应请求终端类型命令：当前终端为={currentTerminalType.Description()}");
    }

    private void ReceiveSocketMessage(Socket client, RequestUdpAddress request)
    {
        _ = Log("收到请求Udp组播地址命令");

        var response = new ResponseUdpAddress()
        {
            TaskId = request.TaskId,
            Ip = UdpHelper.Ip,
            Port = UdpHelper.Port,
        };
        TcpHelper.SendCommand(client, response);

        _ = Log($"响应请求Udp组播地址命令：{response.Ip}:{response.Port}");
    }

    private void ReceiveSocketMessage(Socket client, RequestServiceInfo request)
    {
        var msg = $"收到请求基本信息命令";
        Logger.Logger.Info(msg);
        NotificationManager?.Show(msg);

        var data = MockUtil.GetBaseInfo().Result!;
        data.TaskId = request.TaskId;
        TcpHelper.SendCommand(client, data);

        msg = $"响应基本信息命令：当前操作系统版本号={data.OS}，内存大小={data.MemorySize}GB";
        Logger.Logger.Info(msg);
        NotificationManager?.Show(msg);
    }

    private async void ReceiveSocketMessage(Socket client, RequestProcessList request)
    {
        //var pageCount = MockUtil.GetPageCount(TcpHelper.MockCount, TcpHelper.MockPageSize);
        //var sendCount = 0;
        //for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        //{
        //    var response = new ResponseProcessList
        //    {
        //        TaskId = request.TaskId,
        //        TotalSize = TcpHelper.MockCount,
        //        PageSize = TcpHelper.MockPageSize,
        //        PageCount = pageCount,
        //        PageIndex = pageIndex,
        //        Processes = await MockUtil.MockProcessesAsync(TcpHelper.MockCount, TcpHelper.MockPageSize, pageIndex)
        //    };
        //    sendCount += response.Processes.Count;
        //    TcpHelper.SendCommand(client, response);
        //    await Task.Delay(TimeSpan.FromMilliseconds(10));

        //    var msg = response.TaskId == default ? "推送" : "响应请求";
        //    Logger.Logger.Info(
        //        $"{msg}【{response.PageIndex + 1}/{response.PageCount}】{response.Processes.Count}条({sendCount}/{response.TotalSize})");
        //}
    }

    private void ReceiveSocketMessage(Socket client, ChangeProcessList changeProcess)
    {
        TcpHelper.SendCommand(changeProcess);
    }

    private void ReceiveSocketMessage(Socket client, Heartbeat heartbeat)
    {
        TcpHelper.SendCommand(client, new Heartbeat());
        TcpHelper.HeartbeatTime = DateTime.Now;
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
            Logger.Logger.Info("====TCP推送结构变化通知====");
            return;
        }

        TcpHelper.SendCommand(new UpdateProcessList
        {
            Processes = await MockUtil.MockProcessesAsync(TcpHelper.MockCount, TcpHelper.MockPageSize)
        });
        Logger.Logger.Info("====TCP推送更新通知====");

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
        //if (!UdpHelper.IsRunning) return;

        //var sw = Stopwatch.StartNew();

        //await MockUtil.MockUpdateProcessAsync(TcpHelper.MockCount);
        //sw.Stop();

        //Logger.Logger.Info($"更新模拟实时数据{sw.ElapsedMilliseconds}ms");
    }

    private async void MockSendRealtimeDataAsync(object? sender, ElapsedEventArgs e)
    {
        //if (!UdpHelper.IsRunning) return;

        //var sw = Stopwatch.StartNew();

        //MockUtil.MockUpdateRealtimeProcessPageCount(TcpHelper.MockCount, UdpHelper.PacketMaxSize, out var pageSize,
        //    out var pageCount);

        //var size = 0;
        //for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        //{
        //    if (!UdpHelper.IsRunning) break;

        //    var response = new UpdateRealtimeProcessList
        //    {
        //        TotalSize = TcpHelper.MockCount,
        //        PageSize = pageSize,
        //        PageCount = pageCount,
        //        PageIndex = pageIndex,
        //        Processes = await MockUtil.MockUpdateRealtimeProcessAsync(TcpHelper.MockCount, pageSize,
        //            pageIndex)
        //    };

        //    UdpHelper.SendCommand(response);
        //    TcpHelper.UDPPacketsSentCount++;
        //}

        //Logger.Logger.Info(
        //    $"推送实时数据{TcpHelper.MockCount}条，单包{pageSize}条分{pageCount}包，成功发送{size}字节，{sw.ElapsedMilliseconds}ms");
    }

    private async void MockSendGeneralDataAsync(object? sender, ElapsedEventArgs e)
    {
        //if (!UdpHelper.IsRunning) return;

        //var sw = Stopwatch.StartNew();

        //MockUtil.MockUpdateGeneralProcessPageCount(TcpHelper.MockCount, UdpHelper.PacketMaxSize, out var pageSize,
        //    out var pageCount);

        //var size = 0;
        //for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        //{
        //    if (!UdpHelper.IsRunning) break;

        //    var response = new UpdateGeneralProcessList()
        //    {
        //        TotalSize = TcpHelper.MockCount,
        //        PageSize = pageSize,
        //        PageCount = pageCount,
        //        PageIndex = pageIndex,
        //        Processes = await MockUtil.MockUpdateGeneralProcessAsync(TcpHelper.MockCount, pageSize,
        //            pageIndex)
        //    };
        //    UdpHelper.SendCommand(response);
        //    TcpHelper.UDPPacketsSentCount++;
        //}

        //Logger.Logger.Info(
        //    $"推送一般数据{TcpHelper.MockCount}条，单包{pageSize}条分{pageCount}包，成功发送{size}字节，{sw.ElapsedMilliseconds}ms");
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
            Logger.Logger.Info(msg);
        }
        else if (type == LogType.Error)
        {
            Logger.Logger.Error(msg);
        }

        await ShowNotificationAsync(showNotification, msg, type);
    }

    private async Task ShowNotificationAsync(bool showNotification, string msg, LogType type)
    {
        if (!showNotification) return;

        var notificationType = type switch
        {
            LogType.Warning => NotificationType.Warning,
            LogType.Error => NotificationType.Error,
            _ => NotificationType.Information
        };

        await InvokeAsync(() => NotificationManager?.Show(new Notification(title: "提示", msg, notificationType)));
    }
}