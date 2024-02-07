﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using Messager;
using ReactiveUI;
using SocketDto;
using SocketDto.Message;
using SocketTest.Client.Helpers;
using SocketTest.Client.Models;
using SocketTest.Common;
using SocketTest.Mvvm;

namespace SocketTest.Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly List<ProcessItemModel> _receivedProcesses = new();

        private string? _baseInfo;
        private byte _timestampStartYear;

        private Dictionary<int, ProcessItemModel>? _processIdAndItems;

        private string? _searchKey;

        public Window? Owner { get; set; }
        public ObservableCollection<ProcessItemModel> DisplayProcesses { get; } = new();
        public TcpHelper TcpHelper { get; set; } = new();
        public UdpHelper UdpHelper { get; set; } = new();

        private string? _runTcpCommandContent = "开始运行";

        public string? RunTcpCommandContent
        {
            get => _runTcpCommandContent;
            set => this.RaiseAndSetIfChanged(ref _runTcpCommandContent, value);
        }

        private string? _runUdpCommandContent = "开始运行";

        public string? RunUdpCommandContent
        {
            get => _runUdpCommandContent;
            set => this.RaiseAndSetIfChanged(ref _runUdpCommandContent, value);
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
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        /// <summary>
        ///     刷新数据
        /// </summary>
        public ReactiveCommand<Unit, Unit> RefreshAllCommand { get; }

        public MainWindowViewModel()
        {
            this.WhenAnyValue(x => x.TcpHelper.IsStarted)
                .Select(isStarted => isStarted ? "停止运行" : "开始运行")
                .ToProperty(this, x => x.RunTcpCommandContent);
            this.WhenAnyValue(x => x.UdpHelper.IsStarted)
                .Select(isStarted => isStarted ? "停止运行" : "开始运行")
                .ToProperty(this, x => x.RunUdpCommandContent);

            var isTcpRunning = this.WhenAnyValue(x => x.TcpHelper.IsRunning);
            RefreshCommand = ReactiveCommand.Create(HandleRefreshCommand, isTcpRunning);
            RefreshAllCommand = ReactiveCommand.Create(HandleRefreshAllCommand, isTcpRunning);

            Logger.Logger.Info("连接服务端后获取数据");
        }

        public void HandleConnectTcpCommandAsync()
        {
            if (!TcpHelper.IsStarted)
            {
                TcpHelper.Start();
                Messager.Messenger.Default.Subscribe<TcpMessage>(this, ReadTcpData, ThreadOption.PublisherThread);

                ReceiveTcpData();
                SendHeartbeat();
            }
            else
            {
                Messager.Messenger.Default.Unsubscribe<TcpMessage>(this, ReadTcpData);
                TcpHelper.Stop();
            }
        }

        public void HandleSubscribeUdpMulticastCommand()
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
        }

        private void HandleRefreshCommand()
        {
            if (!TcpHelper.IsRunning)
            {
                Logger.Logger.Error("未连接Tcp服务，无法发送命令");
                return;
            }

            ClearData();
            TcpHelper.SendCommand(new RequestBaseInfo { TaskId = TcpHelper.GetNewTaskId() });
            Logger.Logger.Info("发送刷新命令");
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
            return;
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
                    Logger.Logger.Error($"执行{actionName}异常：{ex.Message}");
                else
                    exceptionAction.Invoke(ex);
            }
        }

        private void Invoke(Action action)
        {
            Dispatcher.UIThread.InvokeAsync(action.Invoke);
        }


        #region 读取Tcp数据

        private void ReceiveTcpData()
        {
            // 开启线程接收数据
            Task.Run(async () =>
            {
                while (!TcpHelper.IsRunning) await Task.Delay(TimeSpan.FromMilliseconds(10));

                HandleRefreshCommand();
            });
        }

        private void ReadTcpData(TcpMessage message)
        {
            var command = message.NetObject;
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

            Logger.Logger.Info(response.TaskId == default ? "收到服务端推送的基本信息" : "收到请求基本信息响应");
            Logger.Logger.Info($"【旧】{oldBaseInfo}");
            Logger.Logger.Info($"【新】{BaseInfo}");

            TcpHelper.SendCommand(new RequestProcessList { TaskId = TcpHelper.GetNewTaskId() });
            Logger.Logger.Info("发送请求进程信息命令");

            ClearData();
        }

        private void ReadTcpData(ResponseProcessList response)
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
            Logger.Logger.Info($"更新数据{response.Processes?.Count}条");
        }

        private void ReadTcpData(Heartbeat response)
        {
        }

        #endregion

        #region 接收Udp数据

        private void ReceiveUdpData()
        {
            Task.Run(async () =>
            {
                while (!UdpHelper.IsRunning) await Task.Delay(TimeSpan.FromMilliseconds(30));

                while (UdpHelper.IsRunning)
                {
                    while (UdpHelper.TryGetResponse(out var response) &&
                           response is UpdateActiveProcessList updateActiveProcess)
                    {
                        ReceiveUdpData(updateActiveProcess);
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(MockConst.UdpDillMilliseconds));
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
}