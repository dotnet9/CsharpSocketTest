using ProcessItem = SocketDto.ProcessItem;

namespace SocketServer.Mock;

public static class MockUtil
{
	public const byte TimestampStartYear = 23;
	private static int _mockCount;
	private static List<ProcessItem>? _mockProcesses;
	private static List<ActiveProcessItem>? _mockUpdateProcesses;

	public static ResponseBaseInfo MockBase(int taskId = default)
	{
		return new ResponseBaseInfo
		{
			TaskId = taskId,
			OS = "Windows 11 专业版",
			MemorySize = 64,
			ProcessorCount = 12,
			DiskSize = 2048,
			NetworkBandwidth = 1024,
			Ips = "192.32.35.23",
			TimestampStartYear = 23,
			LastUpdateTime = TimestampHelper.GetCurrentTimestamp(23)
		};
	}

	public static async Task<List<ProcessItem>> MockProcessesAsync(int totalCount, int pageSize, int pageIndex)
	{
		while (!await MockAllProcessAsync(totalCount))
		{
			// 等待模拟操作完成
			await Task.Delay(TimeSpan.FromMilliseconds(10));
		}

		return _mockProcesses!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
	}

	public static async Task<List<ProcessItem>> MockProcessesAsync(int totalCount, int pageSize)
	{
		while (!await MockAllProcessAsync(totalCount))
		{
			// 等待模拟操作完成
			await Task.Delay(TimeSpan.FromMilliseconds(10));
		}

		var pageCount = GetPageCount(totalCount, pageSize);
		var pageIndex = Random.Shared.Next(0, pageCount);
		return _mockProcesses!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
	}

	private static bool _isMockingAll;

	public static async Task<bool> MockAllProcessAsync(int totalCount)
	{
		if (_isMockingAll)
		{
			return false;
		}

		_isMockingAll = true;

		if (_mockCount == totalCount && _mockProcesses?.Count == totalCount &&
		    _mockUpdateProcesses?.Count == totalCount)
		{
			_isMockingAll = false;
			return true;
		}

		var sw = Stopwatch.StartNew();
		_mockCount = totalCount;

		_mockProcesses?.Clear();
		_mockUpdateProcesses = null;

		_mockProcesses = Enumerable.Range(0, _mockCount).Select(MockProcess).ToList();
		sw.Stop();
		Logger.Info($"模拟{_mockCount}条{sw.ElapsedMilliseconds}ms");
		_mockUpdateProcesses = Enumerable.Range(0, _mockCount)
			.Select(index => new ActiveProcessItem
			{
				ProcessData = new ActiveProcessItemData()
				{
					CPU = MockShort,
					Memory = MockShort,
					Disk = MockShort,
					Network = MockShort,
					GPU = MockShort,
					GPUEngine = (byte)GpuEngine.Gpu03D,
					PowerUsage = (byte)ProcessPowerUsage.Low,
					PowerUsageTrend = (byte)ProcessPowerUsage.Low
				},
				UpdateTime = Timestamp
			})
			.ToList();

		_isMockingAll = false;
		return true;
	}

	private static readonly string MockStr = Lorem.Words(1, 3);
	private static readonly short MockShort = (short)Random.Shared.Next(0, 1000);
	private static readonly uint Timestamp = TimestampStartYear.GetCurrentTimestamp();

	private static ProcessItem MockProcess(int id)
	{
		return new ProcessItem
		{
			PID = id + 1,
			Name = MockStr,
			Publisher = MockStr,
			CommandLine = MockStr,
			ProcessData = new ProcessItemData()
			{
				CPU = MockShort,
				Memory = MockShort,
				Disk = MockShort,
				Network = MockShort,
				GPU = MockShort,
				GPUEngine = (byte)GpuEngine.Gpu03D,
				PowerUsage = (byte)ProcessPowerUsage.Low,
				PowerUsageTrend = (byte)ProcessPowerUsage.Low,
				Type = (byte)ProcessType.Application,
				Status = (byte)ProcessStatus.Pending
			},
			LastUpdateTime = Timestamp,
			UpdateTime = Timestamp
		};
	}


	public static async Task<List<ActiveProcessItem>> MockUpdateProcessAsync(int totalCount, int pageSize,
		int pageIndex)
	{
		while (!await MockAllProcessAsync(totalCount))
		{
			// 等待模拟操作完成
			await Task.Delay(TimeSpan.FromMilliseconds(10));
		}

		return _mockUpdateProcesses!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
	}

	private static readonly Random CustomRandom = new(DateTime.Now.Microsecond);

	public static async Task MockUpdateProcessAsync(int totalCount)
	{
		while (!await MockAllProcessAsync(totalCount))
		{
			// 等待模拟操作完成
			await Task.Delay(TimeSpan.FromMilliseconds(10));
		}

		var cpu = (short)CustomRandom.Next(0, 1000);
		var memory = (short)CustomRandom.Next(0, 1000);
		var disk = (short)CustomRandom.Next(0, 1000);
		var network = (short)CustomRandom.Next(0, 1000);
		var gpu = (short)CustomRandom.Next(0, 1000);
		var powerUsage =
			(byte)CustomRandom.Next(0, Enum.GetNames(typeof(ProcessPowerUsage)).Length);
		var powerUsageTrend =
			(byte)CustomRandom.Next(0, Enum.GetNames(typeof(ProcessPowerUsage)).Length);
		var updateTime = TimestampStartYear.GetCurrentTimestamp();

		_mockUpdateProcesses!.ForEach(process =>
		{
			// 需要重新赋值，才能重新设置buffer
			process.ProcessData = new ActiveProcessItemData()
			{
				CPU = cpu,
				Memory = memory,
				Disk = disk,
				Network = network,
				GPU = gpu,
				PowerUsage =
					powerUsage,
				PowerUsageTrend =
					powerUsageTrend
			};
			process.UpdateTime = updateTime;
		});
	}

	public static int GetPageCount(int totalCount, int pageSize)
	{
		return (totalCount + pageSize - 1) / pageSize;
	}

	public static void MockUpdateActiveProcessPageCount(int totalCount, int packetSize, out int pageSize,
		out int pageCount)
	{
		// sizeof(int)*5为4个数据包基本信息int字段+Processes长度int4个字节
		pageSize = (packetSize - SerializeHelper.PacketHeadLen - sizeof(int) * 5) /
		           ActiveProcessItem.ObjectSize;
		pageCount = GetPageCount(totalCount, pageSize);
	}

	public static int GetDataCount(int totalCount, int pageSize, int pageIndex)
	{
		var dataIndex = pageIndex * pageSize;
		var dataCount = totalCount - dataIndex;
		if (dataCount > pageSize) dataCount = pageSize;

		return dataCount;
	}
}