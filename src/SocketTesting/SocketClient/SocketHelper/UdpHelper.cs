﻿namespace SocketClient.SocketHelper;

public class UdpHelper : BindableBase, ISocketBase
{
	private UdpClient? _client;
	private IPEndPoint _remoteEp = new(IPAddress.Any, 0);
	private readonly BlockingCollection<byte[]> _receivedBuffers = new(new ConcurrentQueue<byte[]>());

	private readonly BlockingCollection<UpdateActiveProcessList> _receivedResponse = new();
	private int _receivedPacketsCount;

	#region 公开属性

	private string _ip = "224.0.0.0";

	/// <summary>
	///     UDP组播IP
	/// </summary>
	public string Ip
	{
		get => _ip;
		set => SetProperty(ref _ip, value);
	}

	private int _port = 9540;

	/// <summary>
	///     UDP组播端口
	/// </summary>
	public int Port
	{
		get => _port;
		set => SetProperty(ref _port, value);
	}

	private bool _isStarted;

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
	///     是否正在运行udp组播订阅
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

	/// <summary>
	/// 已发送UDP包个数
	/// </summary>
	public static int UDPPacketsSentCount { get; set; }

	private string? _receiveCount;

	/// <summary>
	///     UDP接收情况统计
	/// </summary>
	public string? ReceiveCount
	{
		get => _receiveCount;
		set
		{
			if (value != _receiveCount) SetProperty(ref _receiveCount, value);
		}
	}

	#endregion

	#region 公开接口

	public void Start()
	{
		if (IsStarted)
		{
			Logger.Warning("Udp订阅已经开启");
			return;
		}

		IsStarted = true;

		Task.Run(async () =>
		{
			while (IsStarted)
				try
				{
					_client = new UdpClient();
					_client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
					_client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

					// 任意IP+广播端口，0是任意端口
					_client.Client.Bind(new IPEndPoint(IPAddress.Any, Port));

					// 加入组播
					_client.JoinMulticastGroup(IPAddress.Parse(Ip));
					Logger.Info("Udp组播订阅成功");
					IsRunning = true;

					ReceiveData();
					AnalyzeData();
					break;
				}
				catch (Exception ex)
				{
					IsRunning = false;
					Logger.Warning($"运行Udp异常，3秒后将重新运行：{ex.Message}");
					await Task.Delay(TimeSpan.FromSeconds(3));
				}
		});
	}

	public void Stop()
	{
		if (!IsStarted)
		{
			Logger.Warning("Udp订阅已经关闭");
			return;
		}

		IsStarted = false;

		try
		{
			_client?.Close();
			_client = null;
			Logger.Info("停止Udp");
		}
		catch (Exception ex)
		{
			Logger.Warning($"停止Udp异常：{ex.Message}");
		}

		IsRunning = false;
	}

	public void SendCommand(INetObject command)
	{
	}

	public bool TryGetResponse(out INetObject? response)
	{
		var result = _receivedResponse.TryTake(out var updateActiveProcess);
		response = updateActiveProcess;
		return result;
	}

	#endregion

	#region 接收处理数据

	private void ReceiveData()
	{
		Task.Run(async () =>
		{
			while (IsRunning)
				try
				{
					if (_client?.Client == null || _client.Available < 0)
					{
						await Task.Delay(TimeSpan.FromMilliseconds(10));
						continue;
					}

					var data = _client.Receive(ref _remoteEp);
					CountReceivedPackets();
					_receivedBuffers.Add(data);

					ReceiveTime = DateTime.Now;
				}
				catch (SocketException ex)
				{
					Logger.Error(ex.SocketErrorCode == SocketError.Interrupted
						? "Udp中断，停止接收数据！"
						: $"接收Udp数据异常：{ex.Message}");
				}
				catch (Exception ex)
				{
					Logger.Error($"接收Udp数据异常：{ex.Message}");
				}
		});
	}

	private void CountReceivedPackets()
	{
		_receivedPacketsCount++;
		var lostPackets = UDPPacketsSentCount - _receivedPacketsCount;
		var lostPercents = lostPackets * 1.0 / UDPPacketsSentCount;
		ReceiveCount = $"{_receivedPacketsCount}/{UDPPacketsSentCount}（丢包率{lostPercents:P}）";
	}

	private void AnalyzeData()
	{
		Task.Run(async () =>
		{
			while (IsRunning)
			{
				if (!_receivedBuffers.TryTake(out var buffer, TimeSpan.FromMilliseconds(10)))
				{
					await Task.Delay(TimeSpan.FromMilliseconds(50));
					continue;
				}

				var sw = Stopwatch.StartNew();
				var readIndex = 0;
				try
				{
					if (!SerializeHelper.ReadHead(buffer, ref readIndex, out var netObjectInfo)
					    || buffer.Length != netObjectInfo!.BufferLen)
						continue;

					var updateActiveProcess = buffer.DeserializeByNative<UpdateActiveProcessList>();

					_receivedResponse.Add(updateActiveProcess);
				}
				catch (Exception ex)
				{
					Logger.Error($"解析实时数据异常，将放弃处理该包：{ex.Message}");
				}
				finally
				{
					sw.Stop();
					Console.Write($"解析UDP包用时：{sw.ElapsedMilliseconds}ms");
				}
			}
		});
	}

	#endregion
}