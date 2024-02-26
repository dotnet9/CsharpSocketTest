using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using DynamicData;
using Messager;
using ReactiveUI;
using SocketDto;
using SocketDto.AutoCommand;
using SocketDto.Message;
using SocketDto.Requests;
using SocketDto.Response;
using SocketDto.Udp;
using SocketTest.Client.Helpers;
using SocketTest.Client.Models;
using SocketTest.Common;
using SocketTest.Logger.Models;
using SocketTest.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Timers;
using SocketDto.Enums;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace SocketTest.Client.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public WindowNotificationManager? NotificationManager { get; set; }
    private readonly List<ProcessItemModel> _receivedProcesses = new();

    private string? _baseInfo;

    private Dictionary<int, ProcessItemModel>? _processIdAndItems;

    private string? _runTcpCommandContent = "连接服务";

    private string? _searchKey;

    private Timer? _sendDataTimer;
    private byte _timestampStartYear;

    public MainWindowViewModel()
    {
        void ListenProperty()
        {
            this.WhenAnyValue(x => x.TcpHelper.IsRunning)
                .Subscribe(newValue => RunTcpCommandContent = newValue ? "断开服务" : "连接服务");
        }

        void RegisterEvent()
        {
            Messenger.Default.Subscribe<TcpStatusMessage>(this, ReceiveTcpStatusMessage);
            Messenger.Default.Subscribe<SocketMessage>(this, ReceivedSocketMessage);
        }

        void RegisterCommand()
        {
            var isTcpRunning = this.WhenAnyValue(x => x.TcpHelper.IsRunning);
            RefreshCommand = ReactiveCommand.Create(HandleRefreshCommand, isTcpRunning);
            RefreshAllCommand = ReactiveCommand.Create(HandleRefreshAllCommand, isTcpRunning);
        }

        ListenProperty();
        RegisterEvent();
        RegisterCommand();

        Logger.Logger.Info("连接服务端后获取数据");
    }

    public Window? Owner { get; set; }
    public ObservableCollection<ProcessItemModel> DisplayProcesses { get; } = new();
    public TcpHelper TcpHelper { get; set; } = new();
    public UdpHelper UdpHelper { get; set; } = new();

    public string? RunTcpCommandContent
    {
        get => _runTcpCommandContent;
        set => this.RaiseAndSetIfChanged(ref _runTcpCommandContent, value);
    }


    public string? SearchKey
    {
        get => _searchKey;
        set => this.RaiseAndSetIfChanged(ref _searchKey, value);
    }

    /// <summary>
    ///     基本信息
    /// </summary>
    public string? BaseInfo
    {
        get => _baseInfo;
        set => this.RaiseAndSetIfChanged(ref _baseInfo, value);
    }


    /// <summary>
    ///     刷新数据
    /// </summary>
    public ReactiveCommand<Unit, Unit>? RefreshCommand { get; private set; }

    /// <summary>
    ///     刷新数据
    /// </summary>
    public ReactiveCommand<Unit, Unit>? RefreshAllCommand { get; private set; }

    public void HandleConnectTcpCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            TcpHelper.Start();
        }
        else
        {
            TcpHelper.Stop();
        }
    }

    private void HandleRefreshCommand()
    {
        if (!TcpHelper.IsRunning)
        {
            _ = Log("未连接Tcp服务，无法发送命令", LogType.Error);
            return;
        }

        ClearData();
        TcpHelper.SendCommand(new RequestServiceInfo { TaskId = TcpHelper.GetNewTaskId() });
        _ = Log("发送请求服务基本信息命令");
    }

    private void HandleRefreshAllCommand()
    {
        if (!TcpHelper.IsRunning)
        {
            Logger.Logger.Error("未连接Tcp服务，无法发送命令");
            return;
        }

        TcpHelper.SendCommand(new ChangeProcessList());
        Logger.Logger.Info("发送刷新所有客户端命令");
    }

    private IEnumerable<ProcessItemModel> FilterData(IEnumerable<ProcessItemModel> processes)
    {
        return string.IsNullOrWhiteSpace(SearchKey)
            ? processes
            : processes.Where(process =>
                !string.IsNullOrWhiteSpace(process.Name) && process.Name.Contains(SearchKey));
    }

    private void ClearData()
    {
        _receivedProcesses.Clear();
        Invoke(DisplayProcesses.Clear);
    }

    private void SendHeartbeat()
    {
        _sendDataTimer = new Timer();
        _sendDataTimer.Interval = 200;
        _sendDataTimer.Elapsed += MockSendData;
        _sendDataTimer.Start();
    }

    private void MockSendData(object? sender, ElapsedEventArgs e)
    {
        if (!TcpHelper.IsRunning) return;

        TcpHelper.SendCommand(new Heartbeat());
    }

    private void Try(string actionName, Action action, Action<Exception>? exceptionAction = null)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception ex)
        {
            if (exceptionAction == null)
                Logger.Logger.Error($"执行{actionName}异常：{ex.Message}");
            else
                exceptionAction.Invoke(ex);
        }
    }

    private void Invoke(Action action)
    {
        Dispatcher.UIThread.Post(action.Invoke);
    }

    private async Task InvokeAsync(Action action)
    {
        await Dispatcher.UIThread.InvokeAsync(action.Invoke);
    }


    #region 接收事件

    private void ReceiveTcpStatusMessage(TcpStatusMessage message)
    {
        TcpHelper.SendCommand(new RequestTargetType());
        _ = Log("发送命令查询目标终端类型是否是服务端");
    }

    private void ReceiveTcpData()
    {
        // 开启线程接收数据
        Task.Run(async () =>
        {
            while (!TcpHelper.IsRunning) await Task.Delay(TimeSpan.FromMilliseconds(10));

            HandleRefreshCommand();
        });
    }

    /// <summary>
    /// 处理接收的Socket消息
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="Exception"></exception>
    private void ReceivedSocketMessage(SocketMessage message)
    {
        if (message.IsMessage<ResponseTargetType>())
        {
            ReceivedSocketMessage(message.Message<ResponseTargetType>());
        }
        else if (message.IsMessage<ResponseUdpAddress>())
        {
            ReceivedSocketMessage(message.Message<ResponseUdpAddress>());
        }
        else if (message.IsMessage<ResponseServiceInfo>())
        {
            ReceivedSocketMessage(message.Message<ResponseServiceInfo>());
        }
        else if (message.IsMessage<ResponseProcessIDList>())
        {
            ReceivedSocketMessage(message.Message<ResponseProcessIDList>());
        }
        else if (message.IsMessage<ResponseProcessList>())
        {
            ReceivedSocketMessage(message.Message<ResponseProcessList>());
        }
        else if (message.IsMessage<UpdateProcessList>())
        {
            ReceivedSocketMessage(message.Message<UpdateProcessList>());
        }
        else if (message.IsMessage<ChangeProcessList>())
        {
            HandleRefreshCommand();
        }
        else if (message.IsMessage<Heartbeat>())
        {
            ReceivedSocketMessage(message.Message<Heartbeat>());
        }
        else if (message.IsMessage<UpdateRealtimeProcessList>())
        {
            ReceivedSocketMessage(message.MessageByNative<UpdateRealtimeProcessList>());
        }
        else if (message.IsMessage<UpdateGeneralProcessList>())
        {
            ReceivedSocketMessage(message.MessageByNative<UpdateGeneralProcessList>());
        }
    }

    private void ReceivedSocketMessage(ResponseTargetType response)
    {
        var type = (TerminalType)Enum.Parse(typeof(TerminalType), response.Type.ToString());
        if (response.Type == (byte)TerminalType.Server)
        {
            _ = Log($"正确连接{type.Description()}，程序正常运行");

            TcpHelper.SendCommand(new RequestUdpAddress());
            _ = Log("发送命令获取Udp组播地址");

            HandleRefreshCommand();
        }
        else
        {
            _ = Log($"目标终端非服务端(type: {type.Description()})，请检查地址是否配置正确（重点检查端口），即将断开连接", LogType.Error);
        }
    }

    private void ReceivedSocketMessage(ResponseUdpAddress response)
    {
        _ = Log($"收到Udp组播地址=》{response.Ip}:{response.Port}");
    }

    private void ReceivedSocketMessage(ResponseServiceInfo response)
    {
        _timestampStartYear = response.TimestampStartYear;
        var oldBaseInfo = BaseInfo;
        BaseInfo =
            $"更新时间【{response.LastUpdateTime.ToDateTime(response.TimestampStartYear):yyyy:MM:dd HH:mm:ss fff}】：操作系统【{response.OS}】-内存【{response.MemorySize}GB】-处理器【{response.ProcessorCount}个】-硬盘【{response.DiskSize}GB】-带宽【{response.NetworkBandwidth}Mbps】";

        Logger.Logger.Info(response.TaskId == default ? "收到服务端推送的基本信息" : "收到请求基本信息响应");
        Logger.Logger.Info($"【旧】{oldBaseInfo}");
        Logger.Logger.Info($"【新】{BaseInfo}");
        _ = Log(BaseInfo);

        TcpHelper.SendCommand(new RequestProcessIDList() { TaskId = TcpHelper.GetNewTaskId() });
        _ = Log("发送请求进程ID列表命令");

        ClearData();
    }

    private void ReceivedSocketMessage(ResponseProcessIDList response)
    {
        _ = Log($"收到进程ID列表，共{response.IDList?.Length}个进程");

        TcpHelper.SendCommand(new RequestProcessList() { TaskId = TcpHelper.GetNewTaskId() });
        _ = Log("发送请求进程详细信息列表命令");
    }

    private void ReceivedSocketMessage(ResponseProcessList response)
    {
        var processes =
            response.Processes?.ConvertAll(process => new ProcessItemModel(process, _timestampStartYear));
        if (!(processes?.Count > 0)) return;

        _receivedProcesses.AddRange(processes);
        var filterData = FilterData(processes);
        Invoke(() => DisplayProcesses.Add(filterData));
        if (_receivedProcesses.Count == response.TotalSize)
            _processIdAndItems = _receivedProcesses.ToDictionary(process => process.PID);

        var msg = response.TaskId == default ? "收到推送" : "收到请求响应";
        Logger.Logger.Info(
            $"{msg}【{response.PageIndex + 1}/{response.PageCount}】进程{processes.Count}条({_receivedProcesses.Count}/{response.TotalSize})");
    }

    private void ReceivedSocketMessage(UpdateProcessList response)
    {
        if (_processIdAndItems == null) return;

        response.Processes?.ForEach(updateProcess =>
        {
            if (_processIdAndItems.TryGetValue(updateProcess.Pid, out var point))
                point.Update(updateProcess, _timestampStartYear);
            else
                throw new Exception($"收到更新数据包，遇到本地缓存不存在的进程：{updateProcess.Name}");
        });
        Logger.Logger.Info($"更新数据{response.Processes?.Count}条");
    }

    private void ReceivedSocketMessage(Heartbeat response)
    {
    }

    #endregion

    #region 接收Udp数据

    private void ReceivedSocketMessage(UpdateRealtimeProcessList response)
    {
        //var startIndex = response.PageIndex * response.PageSize;
        //for (var i = 0; i < response.Processes!.Count; i++)
        //{
        //    if (_receivedProcesses.Count > startIndex)
        //        _receivedProcesses[startIndex].Update(response.Processes[i]);
        //    else
        //        Console.WriteLine($"【实时】收到更新数据包，遇到本地缓存不存在的进程，索引：{startIndex}");

        //    startIndex++;
        //}

        //Console.WriteLine($"【实时】更新数据{response.Processes?.Count}条");
    }

    private void ReceivedSocketMessage(UpdateGeneralProcessList response)
    {
        //var startIndex = response.PageIndex * response.PageSize;
        //for (var i = 0; i < response.Processes!.Count; i++)
        //{
        //    if (_receivedProcesses.Count > startIndex)
        //        _receivedProcesses[startIndex].Update(response.Processes[i], _timestampStartYear);
        //    else
        //        Console.WriteLine($"【实时】收到更新数据包，遇到本地缓存不存在的进程，索引：{startIndex}");

        //    startIndex++;
        //}

        //Console.WriteLine($"【实时】更新数据{response.Processes?.Count}条");
    }

    #endregion

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