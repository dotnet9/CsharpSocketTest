namespace SocketServer.ViewModels;

public class MainViewModel : BindableBase
{
    private IAsyncCommand? _mockDataCommand;

    private IAsyncCommand? _refreshCommand;

    private IAsyncCommand? _runTcpCommand;

    private IAsyncCommand? _runUdpMulticastCommand;

    private IAsyncCommand? _updateCommand;

    public MainViewModel()
    {
        TcpHelper = new TcpHelper();
        UdpHelper = new UdpHelper(TcpHelper);
    }

    public TcpHelper TcpHelper { get; set; }
    public UdpHelper UdpHelper { get; set; }

    /// <summary>
    ///     连接Tcp服务
    /// </summary>
    public IAsyncCommand RunTcpCommand =>
        _runTcpCommand ??= new AsyncDelegateCommand(HandleRunTcpCommandCommandAsync);

    /// <summary>
    ///     生成模拟数据
    /// </summary>
    public IAsyncCommand MockDataCommand =>
        _mockDataCommand ??= new AsyncDelegateCommand(HandleMockDataCommandAsync);

    /// <summary>
    ///     订阅Udp组播
    /// </summary>
    public IAsyncCommand RunUdpMulticastCommand =>
        _runUdpMulticastCommand ??= new AsyncDelegateCommand(HandleRunUdpMulticastCommandAsync);

    /// <summary>
    ///     刷新数据
    /// </summary>
    public IAsyncCommand RefreshCommand => _refreshCommand ??= new AsyncDelegateCommand(
        HandleRefreshCommandAsync,
        () => TcpHelper.IsRunning).ObservesProperty(() => TcpHelper.IsRunning);

    /// <summary>
    ///     更新数据
    /// </summary>
    public IAsyncCommand UpdateCommand => _updateCommand ??= new AsyncDelegateCommand(
        HandleUpdateCommandAsync,
        () => TcpHelper.IsRunning).ObservesProperty(() => TcpHelper.IsRunning);

    private Task HandleRunTcpCommandCommandAsync()
    {
        if (!TcpHelper.IsStarted)
            TcpHelper.Start();
        else
            TcpHelper.Stop();

        return Task.CompletedTask;
    }

    private Task HandleMockDataCommandAsync()
    {
        MockUtil.MockAllProcess(TcpHelper.MockCount);
        return Task.CompletedTask;
    }

    private Task HandleRunUdpMulticastCommandAsync()
    {
        if (!UdpHelper.IsStarted)
            UdpHelper.Start();
        else
            UdpHelper.Stop();

        return Task.CompletedTask;
    }

    private Task HandleRefreshCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            Logger.Error("未运行Tcp服务，无法发送命令");
            return Task.CompletedTask;
        }

        TcpHelper.UpdateAllData(isUpdateAll: true);
        return Task.CompletedTask;
    }

    private Task HandleUpdateCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            Logger.Error("未运行Tcp服务，无法发送命令");
            return Task.CompletedTask;
        }

        TcpHelper.UpdateAllData(isUpdateAll: false);
        return Task.CompletedTask;
    }


    private void Try(string actionName, Action action, Action<Exception>? exceptionAction = null
    )
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
}