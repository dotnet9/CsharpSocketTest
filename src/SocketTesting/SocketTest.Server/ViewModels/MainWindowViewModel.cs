using SocketTest.Mvvm;
using System.Threading.Tasks;
using System;
using SocketTest.Server.Helpers;
using SocketTest.Server.Mock;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace SocketTest.Server.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public TcpHelper TcpHelper { get; set; }
        public UdpHelper UdpHelper { get; set; }
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


        /// <summary>
        ///     刷新数据
        /// </summary>
        public ReactiveCommand<Unit, Unit>? RefreshCommand { get; }

        /// <summary>
        ///     更新数据
        /// </summary>
        public ReactiveCommand<Unit, Unit>? UpdateCommand { get; }

        public MainWindowViewModel()
        {
            TcpHelper = new TcpHelper();
            UdpHelper = new UdpHelper(TcpHelper);

            this.WhenAnyValue(x => x.TcpHelper.IsStarted)
                .Select(isStarted => isStarted ? "停止运行" : "开始运行")
                .ToProperty(this, x => x.RunTcpCommandContent);
            this.WhenAnyValue(x => x.UdpHelper.IsStarted)
                .Select(isStarted => isStarted ? "停止运行" : "开始运行")
                .ToProperty(this, x => x.RunUdpCommandContent);

            var isTcpRunning = this.WhenAnyValue(x => x.TcpHelper.IsRunning);
            RefreshCommand = ReactiveCommand.Create(HandleRefreshCommandAsync, isTcpRunning);
            UpdateCommand = ReactiveCommand.Create(HandleUpdateCommandAsync, isTcpRunning);

            Logger.Logger.Info("连接服务端后获取数据");
        }

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

            TcpHelper.UpdateAllData(isUpdateAll: true);
            return;
        }

        private void HandleUpdateCommandAsync()
        {
            if (!TcpHelper.IsRunning)
            {
                Logger.Logger.Error("未运行Tcp服务，无法发送命令");
                return;
            }

            TcpHelper.UpdateAllData(isUpdateAll: false);
            return;
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
}