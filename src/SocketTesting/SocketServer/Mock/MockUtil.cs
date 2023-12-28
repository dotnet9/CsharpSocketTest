using ProcessItem = SocketDto.ProcessItem;

namespace SocketServer.Mock;

public static class MockUtil
{
    public const int UdpUpdateMilliseconds = 200;
    public const int UdpSendMilliseconds = 200;
    private static int _mockCount;
    private static List<ProcessItem>? _mockProcesses;
    private static List<ActiveProcessItem>? _mockUpdateProcesses;

    public static ResponseBaseInfo MockBase(int taskId = default)
    {
        return new ResponseBaseInfo
        {
            TaskId = taskId,
            OS = "Windows 11",
            MemorySize = 48,
            ProcessorCount = 8,
            DiskSize = 1024 + 256,
            NetworkBandwidth = 1024,
            Ips = "192.32.35.23",
            ServerName = "Windows server 2021",
            DataCenterLocation = "成都",
            IsRunning = (byte)(ProcessRunningStatus)Enum.Parse(typeof(ProcessRunningStatus),
                Random.Shared.Next(0, Enum.GetNames(typeof(ProcessRunningStatus)).Length).ToString()),
            LastUpdateTime = TimestampHelper.GetCurrentTodayTimestamp()
        };
    }

    public static List<ProcessItem> MockProcesses(int totalCount, int pageSize, int pageIndex)
    {
        MockAllProcess(totalCount);
        return _mockProcesses!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
    }

    public static List<ProcessItem> MockProcesses(int totalCount, int pageSize)
    {
        MockAllProcess(totalCount);
        var pageCount = GetPageCount(totalCount, pageSize);
        var pageIndex = Random.Shared.Next(0, pageCount);
        return _mockProcesses!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
    }

    public static void MockAllProcess(int totalCount)
    {
        if (_mockCount == totalCount && _mockProcesses?.Count == totalCount &&
            _mockUpdateProcesses?.Count == totalCount)
            return;

        var sw = Stopwatch.StartNew();
        _mockCount = totalCount;

        _mockProcesses?.Clear();
        _mockUpdateProcesses = null;

        _mockProcesses = Enumerable.Range(0, _mockCount).Select(MockProcess).ToList();
        sw.Stop();
        Logger.Info($"模拟{_mockCount}条{sw.ElapsedMilliseconds}ms");
        _mockUpdateProcesses = Enumerable.Range(0, _mockCount).Select(index => new ActiveProcessItem { PID = index + 1 })
            .ToList();
        MockUpdateProcess(_mockCount);
    }

    private static readonly string MockStr = Lorem.Words(1, 3);
    private static readonly short MockShort = (short)Random.Shared.Next(0, 1000);
    private static readonly uint Timestamp = TimestampHelper.GetCurrentTodayTimestamp();

    private static ProcessItem MockProcess(int id)
    {
        return new Process
        {
            PID = id + 1,
            Name = MockStr,
            Type = 0,
            Status = 0,
            Publisher = MockStr,
            CommandLine = MockStr,
            CPU = MockShort,
            Memory = MockShort,
            Disk = MockShort,
            Network = MockShort,
            GPU = MockShort,
            GPUEngine = MockStr,
            PowerUsage = 0,
            PowerUsageTrend = 0,
            LastUpdateTime = Timestamp,
            UpdateTime = Timestamp
        };
    }


    public static List<ActiveProcessItem> MockUpdateProcess(int totalCount, int pageSize, int pageIndex)
    {
        MockAllProcess(totalCount);
        return _mockUpdateProcesses!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
    }

    public static void MockUpdateProcess(int totalCount)
    {
        MockAllProcess(totalCount);

        var cpu = (short)Random.Shared.Next(0, 1000);
        var memory = (short)Random.Shared.Next(0, 1000);
        var disk = (short)Random.Shared.Next(0, 1000);
        var network = (short)Random.Shared.Next(0, 1000);
        var gpu = (short)Random.Shared.Next(0, 1000);
        var powerUsage =
            (byte)Random.Shared.Next(0, Enum.GetNames(typeof(ProcessPowerUsage)).Length);
        var powerUsageTrend =
            (byte)Random.Shared.Next(0, Enum.GetNames(typeof(ProcessPowerUsage)).Length);
        var updateTime = TimestampHelper.GetCurrentTodayTimestamp();

        _mockUpdateProcesses!.ForEach(process =>
        {
            process.CPU = cpu;
            process.Memory = memory;
            process.Disk = disk;
            process.Network = network;
            process.GPU = gpu;
            process.PowerUsage =
                powerUsage;
            process.PowerUsageTrend =
                powerUsageTrend;
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
        // sizeof(int)为Processes长度点位4个字节
        pageSize = (packetSize - SerializeHelper.PacketHeadLen - sizeof(int)) /
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