using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CodeWF.EventBus;
using CodeWF.Log.Core;
using CodeWF.NetWrapper.Commands;
using CodeWF.NetWrapper.Helpers;
using CodeWF.Tools.Extensions;
using ReactiveUI;
using SocketDto;
using SocketDto.AutoCommand;
using SocketDto.Enums;
using SocketDto.EventBus;
using SocketDto.Requests;
using SocketDto.Response;
using SocketDto.Udp;
using SocketTest.Client.Extensions;
using SocketTest.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Timers;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace SocketTest.Client.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    public WindowNotificationManager? NotificationManager { get; set; }
    private readonly List<ProcessItemModel> _receivedProcesses = new();

    private int[]? _processIdArray;
    private Dictionary<int, ProcessItemModel>? _processIdAndItems;

    private Timer? _sendDataTimer;
    private int _timestampStartYear;

    public MainWindowViewModel()
    {
        DisplayProcesses = new();

        void RegisterCommand()
        {
            RefreshCommand = ReactiveCommand.CreateFromTask(HandleRefreshCommandAsync);
            RefreshAllCommand = ReactiveCommand.CreateFromTask(HandleRefreshAllCommandAsync);
        }

        EventBus.Default.Subscribe(this);
        RegisterCommand();

        Logger.Info("连接服务端后获取数据");
    }

    #region 属性

    public Window? Owner { get; set; }
    public RangObservableCollection<ProcessItemModel> DisplayProcesses { get; }

    public TcpSocketClient TcpHelper { get; set; } = new();
    public UdpSocketClient UdpHelper { get; set; } = new();

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
    }

    /// <summary>
    ///     UDP组播端口
    /// </summary>
    public int UdpPort
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsRunning
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SearchKey
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     基本信息
    /// </summary>
    public string? BaseInfo
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }


    /// <summary>
    ///     刷新数据
    /// </summary>
    public ReactiveCommand<Unit, Unit>? RefreshCommand { get; private set; }

    /// <summary>
    ///     刷新数据
    /// </summary>
    public ReactiveCommand<Unit, Unit>? RefreshAllCommand { get; private set; }

    #endregion 属性

    public async Task HandleConnectTcpCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            await TcpHelper.ConnectAsync("TCP服务端", TcpIp, TcpPort);
            await RequestTargetTypeAsync();
            IsRunning = true;
        }
        else
        {
            TcpHelper.Stop();
            UdpHelper.Stop();
            UdpHelper.Received -= ReceiveUdpCommand;
            IsRunning = false;
        }
    }

    private async Task HandleRefreshCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            _ = Log("未连接Tcp服务，无法发送命令", LogType.Error);
            return;
        }

        ClearData();
        await TcpHelper.SendCommandAsync(new RequestServiceInfo { TaskId = NetHelper.GetTaskId() });
        _ = Log("发送请求服务基本信息命令");
    }

    private async Task HandleRefreshAllCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            Logger.Error("未连接Tcp服务，无法发送命令");
            return;
        }

        await TcpHelper.SendCommandAsync(new ChangeProcessList());
        Logger.Info("发送刷新所有客户端命令");
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

    private async void MockSendData(object? sender, ElapsedEventArgs e)
    {
        if (!TcpHelper.IsRunning) return;

        await TcpHelper.SendCommandAsync(new Heartbeat());
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
                Logger.Error($"执行{actionName}异常：{ex.Message}");
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


    [EventHandler]
    private async Task ReceiveUdpStatusMessage(ChangeUDPStatusCommand message)
    {
        _ = Log("Udp组播订阅成功！");
    }

    private void ReceiveTcpData()
    {
        // 开启线程接收数据
        Task.Run(async () =>
        {
            while (!TcpHelper.IsRunning) await Task.Delay(TimeSpan.FromMilliseconds(10));

            await HandleRefreshCommandAsync();
        });
    }

    /// <summary>
    /// 处理接收的Socket消息
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="Exception"></exception>
    [EventHandler]
    private async Task ReceivedSocketMessage(SocketCommand message)
    {
        Logger.Info($"Dill command: {message}");
        if (message.IsCommand<ResponseTargetType>())
        {
            await ReceivedSocketMessageAsync(message.GetCommand<ResponseTargetType>());
        }
        else if (message.IsCommand<ResponseUdpAddress>())
        {
            await ReceivedSocketMessageAsync(message.GetCommand<ResponseUdpAddress>());
        }
        else if (message.IsCommand<ResponseServiceInfo>())
        {
            await ReceivedSocketMessageAsync(message.GetCommand<ResponseServiceInfo>());
        }
        else if (message.IsCommand<ResponseProcessIDList>())
        {
            await ReceivedSocketMessageAsync(message.GetCommand<ResponseProcessIDList>());
        }
        else if (message.IsCommand<ResponseProcessList>())
        {
            ReceivedSocketMessage(message.GetCommand<ResponseProcessList>());
        }
        else if (message.IsCommand<UpdateProcessList>())
        {
            ReceivedSocketMessage(message.GetCommand<UpdateProcessList>());
        }
        else if (message.IsCommand<ChangeProcessList>())
        {
            await HandleRefreshCommandAsync();
        }
        else if (message.IsCommand<Heartbeat>())
        {
            ReceivedSocketMessage(message.GetCommand<Heartbeat>());
        }
    }

    private void ReceiveUdpCommand(object? sender, SocketCommand command)
    {
        if (command.IsCommand<UpdateRealtimeProcessList>())
        {
            ReceivedSocketMessage(command.GetCommand<UpdateRealtimeProcessList>());
        }
        else if (command.IsCommand<UpdateGeneralProcessList>())
        {
            ReceivedSocketMessage(command.GetCommand<UpdateGeneralProcessList>());
        }
    }

    private async Task ReceivedSocketMessageAsync(ResponseTargetType response)
    {
        var type = (TerminalType)Enum.Parse(typeof(TerminalType), response.Type.ToString());
        if (response.Type == (byte)TerminalType.Server)
        {
            _ = Log($"正确连接{type.GetDescription()}，程序正常运行");

            await TcpHelper.SendCommandAsync(new RequestUdpAddress());
            _ = Log("发送命令获取Udp组播地址");

            await HandleRefreshCommandAsync();
        }
        else
        {
            _ = Log($"目标终端非服务端(type: {type.GetDescription()})，请检查地址是否配置正确（重点检查端口），即将断开连接", LogType.Error);
        }
    }

    private async Task ReceivedSocketMessageAsync(ResponseUdpAddress response)
    {
        UdpIp = response.Ip;
        UdpPort = response.Port;
        _ = Log($"收到Udp组播地址=》{UdpIp}:{UdpPort}");

        await UdpHelper.ConnectAsync("UDP组播", UdpIp, UdpPort, TcpHelper.LocalEndPoint,
            TcpHelper.SystemId);
        UdpHelper.Received += ReceiveUdpCommand;
        _ = Log("尝试订阅Udp组播");
    }

    private async Task ReceivedSocketMessageAsync(ResponseServiceInfo response)
    {
        _timestampStartYear = response.TimestampStartYear;
        var oldBaseInfo = BaseInfo;
        BaseInfo =
            $"更新时间【{response.LastUpdateTime.FromSpecialUnixTimeSecondsToDateTime(response.TimestampStartYear):yyyy:MM:dd HH:mm:ss fff}】：操作系统【{response.OS}】-内存【{response.MemorySize}GB】-处理器【{response.ProcessorCount}个】-硬盘【{response.DiskSize}GB】-带宽【{response.NetworkBandwidth}Mbps】";

        Logger.Info(response.TaskId == default ? "收到服务端推送的基本信息" : "收到请求基本信息响应");
        Logger.Info($"【旧】{oldBaseInfo}");
        Logger.Info($"【新】{BaseInfo}");
        _ = Log(BaseInfo);

        await TcpHelper.SendCommandAsync(new RequestProcessIDList() { TaskId = NetHelper.GetTaskId() });
        _ = Log("发送请求进程ID列表命令");

        ClearData();
    }

    private async Task ReceivedSocketMessageAsync(ResponseProcessIDList response)
    {
        _processIdArray = response.IDList!;
        _ = Log($"收到进程ID列表，共{_processIdArray.Length}个进程");

        await TcpHelper.SendCommandAsync(new RequestProcessList { TaskId = NetHelper.GetTaskId() });
        _ = Log("发送请求进程详细信息列表命令");
    }

    private void ReceivedSocketMessage(ResponseProcessList response)
    {
        var processes =
            response.Processes?.ConvertAll(process => new ProcessItemModel(process, _timestampStartYear));
        if (!(processes?.Count > 0)) return;

        _receivedProcesses.AddRange(processes);
        var filterData = FilterData(processes);
        Invoke(() => DisplayProcesses.AddRange(filterData));
        if (_receivedProcesses.Count == response.TotalSize)
            _processIdAndItems = _receivedProcesses.ToDictionary(process => process.PID);

        var msg = response.TaskId == default ? "收到推送" : "收到请求响应";
        Logger.Info(
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
        Logger.Info($"更新数据{response.Processes?.Count}条");
    }

    private void ReceivedSocketMessage(Heartbeat response)
    {
    }

    #endregion

    #region 接收Udp数据

    private void ReceivedSocketMessage(UpdateRealtimeProcessList response)
    {
        void LogNotExistProcess(int index)
        {
            Console.WriteLine($"【实时】收到更新数据包，遇到本地缓存不存在的进程，索引：{index}");
        }

        try
        {
            var startIndex = response.PageIndex * response.PageSize;
            var dataCount = response.Cpus.Length / 2;
            for (var i = 0; i < dataCount; i++)
            {
                if (_processIdArray?.Length > startIndex && _processIdAndItems?.Count > startIndex)
                {
                    var processId = _processIdArray[startIndex];
                    if (_processIdAndItems.TryGetValue(processId, out var process))
                    {
                        var cpu = BitConverter.ToInt16(response.Cpus, i * sizeof(Int16));
                        var memory = BitConverter.ToInt16(response.Memories, i * sizeof(Int16));
                        var disk = BitConverter.ToInt16(response.Disks, i * sizeof(Int16));
                        var network = BitConverter.ToInt16(response.Networks, i * sizeof(Int16));
                        process.Update(cpu, memory, disk, network);
                    }
                    else
                    {
                        LogNotExistProcess(startIndex);
                    }
                }
                else
                {
                    LogNotExistProcess(startIndex);
                }

                startIndex++;
            }

            Console.WriteLine($"【实时】更新数据{dataCount}条");
        }
        catch (Exception ex)
        {
            Logger.Error($"【实时】更新数据异常：{ex.Message}");
        }
    }

    private void ReceivedSocketMessage(UpdateGeneralProcessList response)
    {
        void LogNotExistProcess(int index)
        {
            Console.WriteLine($"【实时】收到更新一般数据包，遇到本地缓存不存在的进程，索引：{index}");
        }

        try
        {
            var startIndex = response.PageIndex * response.PageSize;
            var dataCount = response.ProcessStatuses.Length;
            for (var i = 0; i < dataCount; i++)
            {
                if (_processIdArray?.Length > startIndex && _processIdAndItems?.Count > startIndex)
                {
                    var processId = _processIdArray[startIndex];
                    if (_processIdAndItems.TryGetValue(processId, out var process))
                    {
                        var processStatus = response.ProcessStatuses[i];
                        var alarmStatus = response.AlarmStatuses[i];
                        var gpu = BitConverter.ToInt16(response.Gpus, i * sizeof(short));
                        var gpuEngine = response.GpuEngine[i];
                        var powerUsage = response.PowerUsage[i];
                        var powerUsageTrend = response.PowerUsageTrend[i];
                        var updateTime = BitConverter.ToUInt32(response.UpdateTimes, i * sizeof(uint));
                        process.Update(_timestampStartYear, processStatus, alarmStatus, gpu, gpuEngine, powerUsage,
                            powerUsageTrend, updateTime);
                    }
                    else
                    {
                        LogNotExistProcess(startIndex);
                    }
                }
                else
                {
                    LogNotExistProcess(startIndex);
                }

                startIndex++;
            }

            Console.WriteLine($"【实时】更新一般数据{dataCount}条");
        }
        catch (Exception ex)
        {
            Logger.Error($"【实时】更新一般数据异常：{ex.Message}");
        }
    }

    #endregion

    private async Task RequestTargetTypeAsync()
    {
        await TcpHelper.SendCommandAsync(new RequestTargetType() { TaskId = NetHelper.GetTaskId() });
        _ = Log("发送命令查询目标终端类型是否是服务端");
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