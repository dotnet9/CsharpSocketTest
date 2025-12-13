using Avalonia.Threading;
using CodeWF.EventBus;
using CodeWF.Log.Core;
using CodeWF.NetWeaver;
using CodeWF.NetWeaver.Base;
using ReactiveUI;
using SocketDto.EventBus;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CodeWF.NetWrapper.Commands;

namespace SocketTest.Client.Helpers;

public class TcpHelper : ReactiveObject
{
    private Socket? _client;
    public long SystemId { get; private set; } // 服务端标识，TCP数据接收时保存，用于UDP数据包识别

    public readonly BlockingCollection<SocketCommand> _responses = new(new ConcurrentQueue<SocketCommand>());

    #region 公开属性

    /// <summary>
    ///     获取或设置服务器IP地址
    /// </summary>
    public string? ServerIP { get; private set; }

    /// <summary>
    ///     获取或设置服务器端口号
    /// </summary>
    public int ServerPort { get; private set; }


    /// <summary>
    ///     是否正在运行Tcp服务
    /// </summary>
    public bool IsRunning { get; set; }

    #endregion

    #region 公开接口

    private CancellationTokenSource? _connectServer;

    public void Start(string ip, int port)
    {
        ServerIP = ip;
        ServerPort = port;
        _connectServer = new CancellationTokenSource();
        var ipEndPoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
        Task.Run(async () =>
        {
            while (!_connectServer.IsCancellationRequested)
                try
                {
                    _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    await _client.ConnectAsync(ipEndPoint);

                    await Dispatcher.UIThread.InvokeAsync(() => IsRunning = true);

                    _ = Task.Run(ListenForServerAsync);
                    CheckResponse();

                    Logger.Info("连接Tcp服务成功");
                    await EventBus.Default.PublishAsync(new ChangeTCPStatusCommand(true, ServerIP, ServerPort));
                    break;
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    Logger.Warn($"连接TCP服务异常，3秒后将重新连接：{ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
        }, _connectServer.Token);
    }

    public void Stop()
    {
        try
        {
            _connectServer?.Cancel();
            _client?.Close(0);
            Logger.Info("停止Tcp服务");
        }
        catch (Exception ex)
        {
            Logger.Warn($"停止TCP服务异常：{ex.Message}");
        }

        IsRunning = false;
    }

    public void SendCommand(INetObject command)
    {
        if (!IsRunning)
        {
            Logger.Error("Tcp服务未连接，无法发送命令");
            return;
        }

        var buffer = command.Serialize(SystemId);
        _client!.Send(buffer);
        var index = 0;
        buffer.ReadHead(ref index, out var head);
        Logger.Info($"Send(client={_client.RemoteEndPoint},len={buffer.Length})：{head}");
        Logger.Info($"发送命令{command.GetType()}");
    }

    private static int _taskId;

    public static int GetNewTaskId()
    {
        return ++_taskId;
    }

    #endregion

    #region 连接TCP、接收数据

    private async Task ListenForServerAsync()
    {
        while (IsRunning)
        {
            try
            {
                Logger.Info("Listen server");
                var (success, buffer, headInfo) = await _client!.ReadPacketAsync();
                if (!success)
                {
                    continue;
                }

                Logger.Info($"Receive(len={buffer.Length}): {headInfo}");
                SystemId = headInfo!.SystemId;
                _responses.Add(new SocketCommand(headInfo, buffer, _client));
            }
            catch (SocketException ex)
            {
                Logger.Error($"接收数据异常：{ex.Message}");
                break;
            }
            catch (Exception ex)
            {
                Logger.Error($"接收数据异常：{ex.Message}");
            }
        }
    }

    private void CheckResponse()
    {
        Task.Run(async () =>
        {
            while (!IsRunning)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }

            while (IsRunning)
            {
                while (_responses.TryTake(out var message, TimeSpan.FromMilliseconds(10)))
                {
                    Logger.Info($"Send event {message}");
                    await EventBus.Default.PublishAsync(message);
                }
            }
        });
    }

    #endregion
}