﻿using SocketDto.Message;

namespace SocketClient.SocketHelper;

public class TcpHelper : BindableBase, ISocketBase
{
	private Socket? _client;
	public long SystemId { get; } // 服务端标识，TCP数据接收时保存，用于UDP数据包识别

	#region 公开属性

	private string _ip = "127.0.0.1";

	/// <summary>
	///     Tcp服务IP
	/// </summary>
	public string Ip
	{
		get => _ip;
		set => SetProperty(ref _ip, value);
	}

	private int _port = 5000;

	/// <summary>
	///     Tcp服务端口
	/// </summary>
	public int Port
	{
		get => _port;
		set => SetProperty(ref _port, value);
	}

	private bool _isStarted;

	/// <summary>
	///     是否开启Tcp服务
	/// </summary>
	public bool IsStarted
	{
		get => _isStarted;
		set
		{
			if (value != _isStarted) SetProperty(ref _isStarted, value);
		}
	}

	private bool _isRunning;

	/// <summary>
	///     是否正在运行Tcp服务
	/// </summary>
	public bool IsRunning
	{
		get => _isRunning;
		set
		{
			if (value != _isRunning) SetProperty(ref _isRunning, value);
		}
	}

	private DateTime _sendTime;

	/// <summary>
	///     命令发送时间
	/// </summary>
	public DateTime SendTime
	{
		get => _sendTime;
		set
		{
			if (value != _sendTime) SetProperty(ref _sendTime, value);
		}
	}

	private DateTime _receiveTime;

	/// <summary>
	///     响应接收时间
	/// </summary>
	public DateTime ReceiveTime
	{
		get => _receiveTime;
		set
		{
			if (value != _receiveTime) SetProperty(ref _receiveTime, value);
		}
	}

	private DateTime _sendHeartbeatTime;

	/// <summary>
	///     心跳发送时间
	/// </summary>
	public DateTime SendHeartbeatTime
	{
		get => _sendHeartbeatTime;
		set => SetProperty(ref _sendHeartbeatTime, value);
	}

	private DateTime _responseHeartbeatTime;

	/// <summary>
	///     心跳响应时间
	/// </summary>
	public DateTime ResponseHeartbeatTime
	{
		get => _responseHeartbeatTime;
		set => SetProperty(ref _responseHeartbeatTime, value);
	}

	#endregion

	#region 公开接口

	public void Start()
	{
		if (IsStarted)
		{
			Logger.Warning("Tcp连接已经开启");
			return;
		}

		IsStarted = true;

		var ipEndPoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
		Task.Run(async () =>
		{
			while (IsStarted)
				try
				{
					_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					await _client.ConnectAsync(ipEndPoint);
					IsRunning = true;

					ListenForServer();

					Logger.Info("连接Tcp服务成功");
					break;
				}
				catch (Exception ex)
				{
					IsRunning = false;
					Logger.Warning($"连接TCP服务异常，3秒后将重新连接：{ex.Message}");
					await Task.Delay(TimeSpan.FromSeconds(3));
				}
		});
	}

	public void Stop()
	{
		if (!IsStarted)
		{
			Logger.Warning("Tcp连接已经关闭");
			return;
		}

		IsStarted = false;

		try
		{
			_client?.Close(0);
			Logger.Info("停止Tcp服务");
		}
		catch (Exception ex)
		{
			Logger.Warning($"停止TCP服务异常：{ex.Message}");
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
		if (command is Heartbeat)
			SendHeartbeatTime = DateTime.Now;
		else
			Logger.Info($"发送命令{command.GetType()}");
	}

	private static int _taskId;

	public static int GetNewTaskId()
	{
		return ++_taskId;
	}

	#endregion

	#region 连接TCP、接收数据

	private void ListenForServer()
	{
		Task.Run(() =>
		{
			while (IsRunning)
			{
				try
				{
					while (_client!.ReadPacket(out var buffer, out var objectInfo))
					{
						ReceiveTime = DateTime.Now;
						ReceiveResponse(buffer, objectInfo);
					}
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

			return Task.CompletedTask;
		});
	}

	private void ReceiveResponse(byte[] buffer, NetHeadInfo netObjectHeadInfo)
	{
		INetObject command;

		if (netObjectHeadInfo.IsNetObject<ResponseBaseInfo>())
		{
			command = buffer.Deserialize<ResponseBaseInfo>();
		}
		else if (netObjectHeadInfo.IsNetObject<ResponseProcessList>())
		{
			command = buffer.Deserialize<ResponseProcessList>();
		}
		else if (netObjectHeadInfo.IsNetObject<UpdateProcessList>())
		{
			command = buffer.Deserialize<UpdateProcessList>();
		}
		else if (netObjectHeadInfo.IsNetObject<ChangeProcessList>())
		{
			command = buffer.Deserialize<ChangeProcessList>();
		}
		else if (netObjectHeadInfo.IsNetObject<UpdateActiveProcessList>())
		{
			command = buffer.Deserialize<UpdateActiveProcessList>();
		}
		else if (netObjectHeadInfo.IsNetObject<Heartbeat>())
		{
			command = buffer.Deserialize<Heartbeat>();
			ResponseHeartbeatTime = ReceiveTime;
			UdpHelper.UDPPacketsSentCount = (command as Heartbeat)!.UDPPacketsSentCount;
		}
		else
		{
			throw new Exception(
				$"非法数据包：{netObjectHeadInfo}");
		}
		Messager.Messenger.Default.Publish(this, new TcpMessage(this, command));
	}

	#endregion
}