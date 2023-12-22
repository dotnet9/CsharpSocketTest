﻿namespace SocketClient.ViewModels;

public class MainViewModel : BindableBase
{
    private readonly List<ProcessItem> _receivedProcesses = new();

    private string? _baseInfo;

    private IAsyncCommand? _connectTcpCommand;
    private Dictionary<int, ProcessItem>? _processIdAndItems;

    private IAsyncCommand? _refreshCommand;
    private string? _searchKey;

    private IAsyncCommand? _subscribeUdpMulticastCommand;
    public Window? Owner { get; set; }
    public RangObservableCollection<ProcessItem> DisplayProcesses { get; } = new();
    public TcpHelper TcpHelper { get; set; } = new();
    public UdpHelper UdpHelper { get; set; } = new();

    public string? SearchKey
    {
        get => _searchKey;
        set => SetProperty(ref _searchKey, value);
    }

    /// <summary>
    ///     基本信息
    /// </summary>
    public string? BaseInfo
    {
        get => _baseInfo;
        set => SetProperty(ref _baseInfo, value);
    }

    /// <summary>
    ///     连接Tcp服务
    /// </summary>
    public IAsyncCommand ConnectTcpCommand =>
        _connectTcpCommand ??= new AsyncDelegateCommand(HandleConnectTcpCommandAsync);

    /// <summary>
    ///     订阅Udp组播
    /// </summary>
    public IAsyncCommand SubscribeUdpMulticastCommand =>
        _subscribeUdpMulticastCommand ??= new AsyncDelegateCommand(HandleSubscribeUdpMulticastCommand);

    /// <summary>
    ///     刷新数据
    /// </summary>
    public IAsyncCommand RefreshCommand => _refreshCommand ??= new AsyncDelegateCommand(
        HandleRefreshCommand,
        () => TcpHelper.IsRunning).ObservesProperty(() => TcpHelper.IsRunning);

    private Task HandleConnectTcpCommandAsync()
    {
        if (!TcpHelper.IsStarted)
        {
            TcpHelper.Start();

            ReceiveTcpData();
            SendHeartbeat();
        }
        else
        {
            TcpHelper.Stop();
        }

        return Task.CompletedTask;
    }

    private Task HandleSubscribeUdpMulticastCommand()
    {
        if (!UdpHelper.IsStarted)
        {
            UdpHelper.Start();

            ReceiveUdpData();
        }
        else
        {
            UdpHelper.Stop();
        }

        return Task.CompletedTask;
    }

    private Task HandleRefreshCommand()
    {
        if (!TcpHelper.IsRunning)
        {
            Logger.Error("未连接Tcp服务，无法发送命令");
            return Task.CompletedTask;
        }

        ClearData();
        TcpHelper.SendCommand(new RequestBaseInfo { TaskId = TcpHelper.GetNewTaskId() });
        Logger.Info("发送刷新命令");
        return Task.CompletedTask;
    }

    private IEnumerable<ProcessItem> FilterData(IEnumerable<ProcessItem> processes)
    {
        return string.IsNullOrWhiteSpace(SearchKey)
            ? processes
            : processes.Where(process => !string.IsNullOrWhiteSpace(process.Name) && process.Name.Contains(SearchKey));
    }

    private void ClearData()
    {
        _receivedProcesses.Clear();
        Invoke(DisplayProcesses.Clear);
    }

    private void SendHeartbeat()
    {
        Task.Run(() =>
        {
            while (!TcpHelper.IsRunning) Thread.Sleep(TimeSpan.FromMilliseconds(30));

            while (TcpHelper.IsRunning)
            {
                TcpHelper.SendCommand(new Heartbeat());
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        });
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
        Owner?.Dispatcher.Invoke(action);
    }


    #region 读取Tcp数据

    private void ReceiveTcpData()
    {
        // 开启线程接收数据
        Task.Run(() =>
        {
            while (!TcpHelper.IsRunning) Thread.Sleep(TimeSpan.FromMilliseconds(50));

            HandleRefreshCommand();

            while (TcpHelper.IsRunning)
            {
                Try("读取TCP数据", ReadTcpData, ex => Logger.Error($"循环处理数据异常：{ex.Message}"));

                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }
        });
    }

    private void ReadTcpData()
    {
        if (!TcpHelper.TryGetResponse(out var command)) return;

        switch (command)
        {
            case ResponseBaseInfo responseBase:
                ReadTcpData(responseBase);
                break;
            case ResponseProcess responseProcess:
                ReadTcpData(responseProcess);
                break;
            case UpdateProcess updateProcess:
                ReadTcpData(updateProcess);
                break;
            case ChangeProcess _:
                HandleRefreshCommand();
                break;
            case Heartbeat responseHeartbeat:
                ReadTcpData(responseHeartbeat);
                break;
            default:
                throw new Exception($"视图未处理新的数据包{command!.GetType().Name}");
        }
    }

    private void ReadTcpData(ResponseBaseInfo response)
    {
        var oldBaseInfo = BaseInfo;
        BaseInfo =
            $"更新时间【{response.LastUpdateTime.ToDateTime():yyyy:MM:dd HH:mm:ss fff}】：数据中心【{response.DataCenterLocation}】-操作系统【{response.OperatingSystemType}】-内存【{ByteSizeConverter.FormatMB(response.MemorySize)}】-处理器个数【{response.ProcessorCount}】-硬盘【{ByteSizeConverter.FormatGB(response.TotalDiskSpace)}】-带宽【{response.NetworkBandwidth}Mbps】";

        Logger.Info(response.TaskId == default ? "收到服务端推送的基本信息" : "收到请求基本信息响应");
        Logger.Info($"【旧】{oldBaseInfo}");
        Logger.Info($"【新】{BaseInfo}");

        TcpHelper.SendCommand(new RequestProcess { TaskId = TcpHelper.GetNewTaskId() });
        Logger.Info("发送请求进程信息命令");

        ClearData();
    }

    private void ReadTcpData(ResponseProcess response)
    {
        var processes = response.Processes?.ConvertAll(process => new ProcessItem(process));
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

    private void ReadTcpData(UpdateProcess response)
    {
        if (_processIdAndItems == null) return;

        response.Processes?.ForEach(updateProcess =>
        {
            if (_processIdAndItems.TryGetValue(updateProcess.PID, out var point))
                point.Update(updateProcess);
            else
                throw new Exception($"收到更新数据包，遇到本地缓存不存在的进程：{updateProcess.Name}");
        });
        Logger.Info($"更新数据{response.Processes?.Count}条");
    }

    private void ReadTcpData(Heartbeat response)
    {
    }

    #endregion

    #region 接收Udp数据

    private void ReceiveUdpData()
    {
        Task.Run(() =>
        {
            while (!UdpHelper.IsRunning) Thread.Sleep(TimeSpan.FromMilliseconds(30));

            while (UdpHelper.IsRunning)
            {
                var allUpdateProcesses = new List<UpdateActiveProcess>();
                while (UdpHelper.TryGetResponse(out var response) &&
                       response is UpdateActiveProcess updateActiveProcess)
                {
                    allUpdateProcesses.Add(updateActiveProcess);
                    if (allUpdateProcesses.Count > 50) break;
                }

                if (allUpdateProcesses.Count > 0)
                    foreach (var item in allUpdateProcesses)
                        try
                        {
                            ReceiveUdpData(item);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"更新点实时数据异常：{ex.Message}");
                        }

                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }
        });
    }

    private void ReceiveUdpData(UpdateActiveProcess response)
    {
        response.Processes?.ForEach(updateProcess =>
        {
            if (_processIdAndItems != null && _processIdAndItems.TryGetValue(updateProcess.PID, out var point))
                point.Update(updateProcess);
            else
                Console.WriteLine($"【实时】收到更新数据包，遇到本地缓存不存在的进程：{updateProcess.PID}");
        });
        Console.WriteLine($"【实时】更新数据{response.Processes?.Count}条");
    }

    #endregion
}