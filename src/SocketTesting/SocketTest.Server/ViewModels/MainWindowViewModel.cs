using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SocketTest.Mvvm;
using SocketTest.Server.Helpers;
using SocketTest.Server.Mock;

namespace SocketTest.Server.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string? _runTcpCommandContent = "开启服务";

    private string? _runUdpCommandContent = "开启服务";

    public MainWindowViewModel()
    {
        TcpHelper = new TcpHelper();
        UdpHelper = new UdpHelper(TcpHelper);

        this.WhenAnyValue(x => x.TcpHelper.IsStarted)
            .Select(isStarted => isStarted ? "停止服务" : "开启服务")
            .ToProperty(this, x => x.RunTcpCommandContent);
        this.WhenAnyValue(x => x.UdpHelper.IsStarted)
            .Select(isStarted => isStarted ? "停止服务" : "开启服务")
            .ToProperty(this, x => x.RunUdpCommandContent);

        var isTcpRunning = this.WhenAnyValue(x => x.TcpHelper.IsRunning);
        RefreshCommand = ReactiveCommand.Create(HandleRefreshCommandAsync, isTcpRunning);
        UpdateCommand = ReactiveCommand.Create(HandleUpdateCommandAsync, isTcpRunning);

        Logger.Logger.Info("连接服务端后获取数据");
    }

    public TcpHelper TcpHelper { get; set; }
    public UdpHelper UdpHelper { get; set; }

    public string? RunTcpCommandContent
    {
        get => _runTcpCommandContent;
        set => this.RaiseAndSetIfChanged(ref _runTcpCommandContent, value);
    }

    public string? RunUdpCommandContent
    {
        get => _runUdpCommandContent;
        set => this.RaiseAndSetIfChanged(ref _runUdpCommandContent, value);
    }


    /// <summary>
    ///     刷新数据
    /// </summary>
    public ReactiveCommand<Unit, Unit>? RefreshCommand { get; }

    /// <summary>
    ///     更新数据
    /// </summary>
    public ReactiveCommand<Unit, Unit>? UpdateCommand { get; }

    public Task HandleRunTcpCommandCommandAsync()
    {
        if (!TcpHelper.IsStarted)
            TcpHelper.Start();
        else
            TcpHelper.Stop();

        return Task.CompletedTask;
    }

    public void HandleMockDataCommandAsync()
    {
        Task.Run(async () => { await MockUtil.MockAllProcessAsync(TcpHelper.MockCount); });
    }

    public void HandleRunUdpMulticastCommandAsync()
    {
        if (!UdpHelper.IsStarted)
            UdpHelper.Start();
        else
            UdpHelper.Stop();
    }

    private void HandleRefreshCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            Logger.Logger.Error("未运行Tcp服务，无法发送命令");
            return;
        }

        TcpHelper.UpdateAllData(true);
    }

    private void HandleUpdateCommandAsync()
    {
        if (!TcpHelper.IsRunning)
        {
            Logger.Logger.Error("未运行Tcp服务，无法发送命令");
            return;
        }

        TcpHelper.UpdateAllData(false);
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
                Logger.Logger.Error($"执行{actionName}异常：{ex.Message}");
            else
                exceptionAction.Invoke(ex);
        }
    }
}