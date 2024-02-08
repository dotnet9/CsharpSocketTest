using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Timers;
using Messager;
using ReactiveUI;
using SocketDto;
using SocketDto.Message;
using SocketNetObject;
using SocketNetObject.Models;
using SocketTest.Common;
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

        Messenger.Default.Subscribe<SocketMessage>(this, ReceiveSocketMessage, ThreadOption.PublisherThread);

        this.WhenAnyValue(x => x.TcpHelper.IsStarted)
            .Subscribe(newValue => RunTcpCommandContent = newValue ? "停止服务" : "开启服务");
        this.WhenAnyValue(x => x.UdpHelper.IsStarted)
            .Subscribe(newValue => RunUdpCommandContent = newValue ? "停止服务" : "开启服务");

        var isTcpRunning = this.WhenAnyValue(x => x.TcpHelper.IsRunning);
        RefreshCommand = ReactiveCommand.Create(HandleRefreshCommandAsync, isTcpRunning);
        UpdateCommand = ReactiveCommand.Create(HandleUpdateCommandAsync, isTcpRunning);
        MockUpdate();
        MockSendData();

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

    private void ReceiveSocketMessage(SocketMessage message)
    {
        if (message.IsMessage<RequestBaseInfo>())
        {
            ReceiveSocketMessage(message.Client!, message.Message<RequestBaseInfo>());
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

    private void ReceiveSocketMessage(Socket client, RequestBaseInfo request)
    {
        TcpHelper.SendCommand(client, MockUtil.MockBase(request.TaskId));
    }

    private async void ReceiveSocketMessage(Socket client, RequestProcessList request)
    {
        var pageCount = MockUtil.GetPageCount(TcpHelper.MockCount, TcpHelper.MockPageSize);
        var sendCount = 0;
        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            var response = new ResponseProcessList
            {
                TaskId = request.TaskId,
                TotalSize = TcpHelper.MockCount,
                PageSize = TcpHelper.MockPageSize,
                PageCount = pageCount,
                PageIndex = pageIndex,
                Processes = await MockUtil.MockProcessesAsync(TcpHelper.MockCount, TcpHelper.MockPageSize, pageIndex)
            };
            sendCount += response.Processes.Count;
            TcpHelper.SendCommand(client, response);
            await Task.Delay(TimeSpan.FromMilliseconds(10));

            var msg = response.TaskId == default ? "推送" : "响应请求";
            Logger.Logger.Info(
                $"{msg}【{response.PageIndex + 1}/{response.PageCount}】{response.Processes.Count}条({sendCount}/{response.TotalSize})");
        }
    }

    private void ReceiveSocketMessage(Socket client, ChangeProcessList changeProcess)
    {
        TcpHelper.SendCommand(changeProcess);
    }

    private void ReceiveSocketMessage(Socket client, Heartbeat heartbeat)
    {
        TcpHelper.SendCommand(client, new Heartbeat { UDPPacketsSentCount = TcpHelper.UDPPacketsSentCount });
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
        if (!UdpHelper.IsRunning) return;

        var sw = Stopwatch.StartNew();

        await MockUtil.MockUpdateProcessAsync(TcpHelper.MockCount);
        sw.Stop();

        Logger.Logger.Info($"更新模拟实时数据{sw.ElapsedMilliseconds}ms");
    }

    private async void MockSendRealtimeDataAsync(object? sender, ElapsedEventArgs e)
    {
        if (!UdpHelper.IsRunning) return;

        var sw = Stopwatch.StartNew();

        MockUtil.MockUpdateRealtimeProcessPageCount(TcpHelper.MockCount, UdpHelper.PacketMaxSize, out var pageSize,
            out var pageCount);

        var size = 0;
        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            if (!UdpHelper.IsRunning) break;

            var response = new UpdateRealtimeProcessList
            {
                TotalSize = TcpHelper.MockCount,
                PageSize = pageSize,
                PageCount = pageCount,
                PageIndex = pageIndex,
                Processes = await MockUtil.MockUpdateRealtimeProcessAsync(TcpHelper.MockCount, pageSize,
                    pageIndex)
            };

            UdpHelper.SendCommand(response);
            TcpHelper.UDPPacketsSentCount++;
        }

        Logger.Logger.Info(
            $"推送实时数据{TcpHelper.MockCount}条，单包{pageSize}条分{pageCount}包，成功发送{size}字节，{sw.ElapsedMilliseconds}ms");
    }

    private async void MockSendGeneralDataAsync(object? sender, ElapsedEventArgs e)
    {
        if (!UdpHelper.IsRunning) return;

        var sw = Stopwatch.StartNew();

        MockUtil.MockUpdateGeneralProcessPageCount(TcpHelper.MockCount, UdpHelper.PacketMaxSize, out var pageSize,
            out var pageCount);

        var size = 0;
        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            if (!UdpHelper.IsRunning) break;

            var response = new UpdateGeneralProcessList()
            {
                TotalSize = TcpHelper.MockCount,
                PageSize = pageSize,
                PageCount = pageCount,
                PageIndex = pageIndex,
                Processes = await MockUtil.MockUpdateGeneralProcessAsync(TcpHelper.MockCount, pageSize,
                    pageIndex)
            };
            UdpHelper.SendCommand(response);
            TcpHelper.UDPPacketsSentCount++;
        }

        Logger.Logger.Info(
            $"推送一般数据{TcpHelper.MockCount}条，单包{pageSize}条分{pageCount}包，成功发送{size}字节，{sw.ElapsedMilliseconds}ms");
    }

    #endregion
}