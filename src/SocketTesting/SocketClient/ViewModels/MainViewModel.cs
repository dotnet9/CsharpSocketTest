using System.Timers;

namespace SocketClient.ViewModels;

public class MainViewModel : BindableBase
{
    private readonly List<ProcessItemModel> _receivedProcesses = new();

    private string? _baseInfo;
    private byte _timestampStartYear;

    private IAsyncCommand? _connectTcpCommand;
    private Dictionary<int, ProcessItemModel>? _processIdAndItems;

    private IAsyncCommand? _refreshCommand;
    private string? _searchKey;

    private IAsyncCommand? _subscribeUdpMulticastCommand;
    public Window? Owner { get; set; }
    public RangObservableCollection<ProcessItemModel> DisplayProcesses { get; } = new();
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

    private IEnumerable<ProcessItemModel> FilterData(IEnumerable<ProcessItemModel> processes)
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

    private System.Timers.Timer? _sendDataTimer;

    private void SendHeartbeat()
    {
        _sendDataTimer = new System.Timers.Timer();
        _sendDataTimer.Interval = 200;
        _sendDataTimer.Elapsed += MockSendData;
        _sendDataTimer.Start();
    }

    private void MockSendData(object? sender, ElapsedEventArgs e)
    {
        if (!TcpHelper.IsRunning)
        {
            return;
        }

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
                Logger.Error($"执行{actionName}异常：{ex.Message}");
            else
                exceptionAction.Invoke(ex);
        }
    }

    private void Invoke(Action action)
    {
        Owner?.Dispatcher.BeginInvoke(action.Invoke);
    }


    #region 读取Tcp数据

    private void ReceiveTcpData()
    {
        // 开启线程接收数据
        Task.Run(() =>
        {
            while (!TcpHelper.IsRunning) Thread.Sleep(TimeSpan.FromMilliseconds(10));

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
            case ResponseProcessList responseProcess:
                ReadTcpData(responseProcess);
                break;
            case UpdateProcessList updateProcess:
                ReadTcpData(updateProcess);
                break;
            case ChangeProcessList _:
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
        _timestampStartYear = response.TimestampStartYear;
        var oldBaseInfo = BaseInfo;
        BaseInfo =
            $"更新时间【{response.LastUpdateTime.ToDateTime(response.TimestampStartYear):yyyy:MM:dd HH:mm:ss fff}】：操作系统【{response.OS}】-内存【{response.MemorySize}GB】-处理器【{response.ProcessorCount}个】-硬盘【{response.DiskSize}GB】-带宽【{response.NetworkBandwidth}Mbps】";

        Logger.Info(response.TaskId == default ? "收到服务端推送的基本信息" : "收到请求基本信息响应");
        Logger.Info($"【旧】{oldBaseInfo}");
        Logger.Info($"【新】{BaseInfo}");

        TcpHelper.SendCommand(new RequestProcessList { TaskId = TcpHelper.GetNewTaskId() });
        Logger.Info("发送请求进程信息命令");

        ClearData();
    }

    private void ReadTcpData(ResponseProcessList response)
    {
        var processes = response.Processes?.ConvertAll(process => new ProcessItemModel(process, _timestampStartYear));
        if (!(processes?.Count > 0)) return;

        _receivedProcesses.AddRange(processes);
        var filterData = FilterData(processes);
        Invoke(() => DisplayProcesses.Add(filterData));
        if (_receivedProcesses.Count == response.TotalSize)
            _processIdAndItems = _receivedProcesses.ToDictionary(process => process.PID);

        var msg = response.TaskId == default ? "收到推送" : "收到请求响应";
        Logger.Info(
            $"{msg}【{response.PageIndex + 1}/{response.PageCount}】进程{processes.Count}条({_receivedProcesses.Count}/{response.TotalSize})");
    }

    private void ReadTcpData(UpdateProcessList response)
    {
        if (_processIdAndItems == null) return;

        response.Processes?.ForEach(updateProcess =>
        {
            if (_processIdAndItems.TryGetValue(updateProcess.PID, out var point))
                point.Update(updateProcess, _timestampStartYear);
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
                var allUpdateProcesses = new List<UpdateActiveProcessList>();
                while (UdpHelper.TryGetResponse(out var response) &&
                       response is UpdateActiveProcessList updateActiveProcess)
                {
                    allUpdateProcesses.Add(updateActiveProcess);
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

                Thread.Sleep(TimeSpan.FromMilliseconds(500));
            }
        });
    }

    private void ReceiveUdpData(UpdateActiveProcessList response)
    {
        var startIndex = response.PageIndex * response.PageSize;
        for (var i = 0; i < response.Processes!.Count; i++)
        {
            if (_receivedProcesses.Count > startIndex)
            {
                _receivedProcesses[startIndex].Update(response.Processes[i], _timestampStartYear);
            }
            else
            {
                Console.WriteLine($"【实时】收到更新数据包，遇到本地缓存不存在的进程，索引：{startIndex}");
            }

            startIndex++;
        }

        Console.WriteLine($"【实时】更新数据{response.Processes?.Count}条");
    }

    #endregion
}