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
using CodeWF.NetWrapper.Helpers;

namespace SocketTest.Client.Helpers;

public class TcpSocketClient : ReactiveObject
{
    private Socket? _client;

    public readonly BlockingCollection<SocketCommand> _responses = new(new ConcurrentQueue<SocketCommand>());

    #region 公开属性

    /// <summary>
    ///     服务标识，用以区分多个服务
    /// </summary>
    public string? ServerMark { get; private set; }

    /// <summary>
    ///     获取或设置服务器IP地址
    /// </summary>
    public string? ServerIP { get; private set; }

    /// <summary>
    ///     服务端标识，TCP数据接收时保存，用于UDP数据包识别
    /// </summary>
    public long SystemId { get; private set; }

    /// <summary>
    ///     获取或设置服务器端口号
    /// </summary>
    public int ServerPort { get; private set; }


    /// <summary>
    ///     是否正在运行Tcp服务
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// 本地端点连接信息
    /// </summary>
    public string? LocalEndPoint { get; set; }

    #endregion

    #region 公开接口

    private CancellationTokenSource? _connectServer;

    public async Task<(bool IsSuccess, string? ErrorMessage)> ConnectAsync(string serverMark, string ip, int port)
    {
        ServerMark = serverMark;
        ServerIP = ip;
        ServerPort = port;
        _connectServer = new CancellationTokenSource();
        var ipEndPoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
        try
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _client.ConnectAsync(ipEndPoint);

            await Dispatcher.UIThread.InvokeAsync(() => IsRunning = true);

            _ = Task.Run(ListenForServerAsync);
            _ = Task.Run(CheckResponseAsync);

            LocalEndPoint = _client.LocalEndPoint?.ToString();
            Logger.Info("连接Tcp服务成功");
            await EventBus.Default.PublishAsync(new ChangeTCPStatusCommand(true, ServerIP, ServerPort));
            return (IsSuccess: true, ErrorMessage: null);
        }
        catch (Exception ex)
        {
            IsRunning = false;
            LocalEndPoint = null;
            Logger.Error($"{ServerMark} 连接异常", ex, $"{ServerMark} 连接异常，详细信息请查看日志文件");
            return (IsSuccess: false, ErrorMessage: $"{ServerMark} 连接异常，异常信息：{ex.Message}");
        }
    }

    public void Stop()
    {
        try
        {
            _connectServer?.Cancel();
            _client?.CloseSocket();
            LocalEndPoint = null;
            Logger.Info("停止Tcp服务");
        }
        catch (Exception ex)
        {
            Logger.Warn($"停止TCP服务异常：{ex.Message}");
        }

        IsRunning = false;
    }

    /// <summary>
    ///     异步发送命令到服务器
    /// </summary>
    /// <param name="command">要发送的命令对象</param>
    /// <exception cref="Exception">当客户端未连接时抛出</exception>
    public async Task SendCommandAsync(INetObject command)
    {
        if (!IsRunning || !_client.Connected) throw new Exception($"{ServerMark} 未连接，无法发送命令");

        var buffer = command.Serialize(SystemId);
        await _client!.SendAsync(buffer);
    }

    #endregion

    #region 连接TCP、接收数据

    /// <summary>
    ///     监听服务器发送的数据
    /// </summary>
    private async Task ListenForServerAsync()
    {
        while (IsRunning)
            try
            {
                var (success, buffer, headInfo) = await _client!.ReadPacketAsync();
                if (!success) break;

                SystemId = headInfo!.SystemId;
                _responses.Add(new SocketCommand(headInfo, buffer, _client));
            }
            catch (SocketException ex)
            {
                Logger.Error($"{ServerMark} 处理接收数据异常", ex, $"{ServerMark} 处理接收数据异常，详细信息请查看日志文件");
                break;
            }
            catch (Exception ex)
            {
                if (IsRunning) Logger.Error($"{ServerMark} 处理接收数据异常", ex, $"{ServerMark} 处理接收数据异常，详细信息请查看日志文件");

                break;
            }
    }

    /// <summary>
    ///     检查响应命令队列
    /// </summary>
    private async Task CheckResponseAsync()
    {
        while (!IsRunning) await Task.Delay(TimeSpan.FromMilliseconds(10));

        while (IsRunning)
        {
            while (_responses.TryTake(out var command, TimeSpan.FromMilliseconds(10)))
                await EventBus.Default.PublishAsync(command);
        }
    }

    #endregion
}