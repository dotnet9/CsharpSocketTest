using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LoremNET;
using SocketDto;
using SocketNetObject;
using SocketTest.Common;
using ProcessItem = SocketDto.ProcessItem;

namespace SocketTest.Server.Mock;

public static class MockUtil
{
    public const byte TimestampStartYear = 23;
    private static int _mockCount;
    private static List<ProcessItem>? _mockProcesses;
    private static List<RealtimeProcessItem>? _mockUpdateRealtimeProcesses;
    private static List<GeneralProcessItem>? _mockUpdateGeneralProcesses;

    private static bool _isMockingAll;

    private static readonly string MockStr = Lorem.Words(1, 3);
    private static readonly short MockShort = (short)Random.Shared.Next(0, 1000);
    private static readonly uint Timestamp = TimestampStartYear.GetCurrentTimestamp();

    private static readonly Random CustomRandom = new(DateTime.Now.Microsecond);

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
            // 等待模拟操作完成
            await Task.Delay(TimeSpan.FromMilliseconds(10));

        return _mockProcesses!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
    }

    public static async Task<List<ProcessItem>> MockProcessesAsync(int totalCount, int pageSize)
    {
        while (!await MockAllProcessAsync(totalCount))
            // 等待模拟操作完成
            await Task.Delay(TimeSpan.FromMilliseconds(10));

        var pageCount = GetPageCount(totalCount, pageSize);
        var pageIndex = Random.Shared.Next(0, pageCount);
        return _mockProcesses!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
    }

    public static async Task<bool> MockAllProcessAsync(int totalCount)
    {
        if (_isMockingAll) return false;

        _isMockingAll = true;

        if (_mockCount == totalCount && _mockProcesses?.Count == totalCount &&
            _mockUpdateRealtimeProcesses?.Count == totalCount)
        {
            _isMockingAll = false;
            return true;
        }

        var sw = Stopwatch.StartNew();
        _mockCount = totalCount;

        _mockProcesses?.Clear();
        _mockUpdateRealtimeProcesses = null;
        _mockUpdateGeneralProcesses = null;

        _mockProcesses = Enumerable.Range(0, _mockCount).Select(MockProcess).ToList();
        sw.Stop();
        Logger.Logger.Info($"模拟{_mockCount}条{sw.ElapsedMilliseconds}ms");
        _mockUpdateRealtimeProcesses = Enumerable.Range(0, _mockCount)
            .Select(index => new RealtimeProcessItem()
            {
                ProcessData = new RealtimeProcessItemData()
                {
                    Cpu = MockShort,
                    Memory = MockShort,
                    Disk = MockShort,
                    Network = MockShort
                },
            })
            .ToList();

        _mockUpdateGeneralProcesses = Enumerable.Range(0, _mockCount)
            .Select(index => new GeneralProcessItem()
            {
                ProcessData = new GeneralProcessItemData()
                {
                    ProcessStatus = (byte)(DateTime.Now.Microsecond % 5),
                    AlarmStatus = (byte)(DateTime.Now.Microsecond % 8),
                    Gpu = MockShort,
                    GpuEngine = (byte)GpuEngine.Gpu03D,
                    PowerUsage = (byte)ProcessPowerUsage.Low,
                    PowerUsageTrend = (byte)ProcessPowerUsage.Low
                },
                UpdateTime = Timestamp
            })
            .ToList();

        _isMockingAll = false;
        return await Task.FromResult(true);
    }

    private static ProcessItem MockProcess(int id)
    {
        return new ProcessItem
        {
            PID = id + 1,
            Name = MockStr,
            Publisher = MockStr,
            CommandLine = MockStr,
            ProcessData = new ProcessItemData
            {
                Cpu = MockShort,
                Memory = MockShort,
                Disk = MockShort,
                Network = MockShort,
                Gpu = MockShort,
                GpuEngine = (byte)GpuEngine.Gpu03D,
                PowerUsage = (byte)ProcessPowerUsage.Low,
                PowerUsageTrend = (byte)ProcessPowerUsage.Low,
                Type = (byte)ProcessType.Application,
                Status = (byte)ProcessStatus.Ready
            },
            LastUpdateTime = Timestamp,
            UpdateTime = Timestamp
        };
    }


    public static async Task<List<RealtimeProcessItem>>
        MockUpdateRealtimeProcessAsync(int totalCount, int pageSize,
            int pageIndex)
    {
        while (!await MockAllProcessAsync(totalCount))
            // 等待模拟操作完成
            await Task.Delay(TimeSpan.FromMilliseconds(10));

        return _mockUpdateRealtimeProcesses!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
    }

    public static async Task<List<GeneralProcessItem>>
        MockUpdateGeneralProcessAsync(int totalCount, int pageSize,
            int pageIndex)
    {
        while (!await MockAllProcessAsync(totalCount))
            // 等待模拟操作完成
            await Task.Delay(TimeSpan.FromMilliseconds(10));

        return _mockUpdateGeneralProcesses!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
    }

    public static async Task MockUpdateProcessAsync(int totalCount)
    {
        while (!await MockAllProcessAsync(totalCount))
            // 等待模拟操作完成
            await Task.Delay(TimeSpan.FromMilliseconds(10));

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

        _mockUpdateRealtimeProcesses!.ForEach(process =>
        {
            // 需要重新赋值，才能重新设置buffer
            process.ProcessData = new RealtimeProcessItemData()
            {
                Cpu = cpu,
                Memory = memory,
                Disk = disk,
                Network = network,
            };
        });
        _mockUpdateGeneralProcesses!.ForEach(process =>
        {
            // 需要重新赋值，才能重新设置buffer
            process.ProcessData = new GeneralProcessItemData()
            {
                ProcessStatus = (byte)(DateTime.Now.Millisecond % 5),
                AlarmStatus = (byte)(DateTime.Now.Millisecond % 8),
                Gpu = gpu,
                GpuEngine = (byte)(DateTime.Now.Millisecond % 2),
                PowerUsage = powerUsage,
                PowerUsageTrend = powerUsageTrend
            };
            process.UpdateTime = updateTime;
        });
    }

    public static int GetPageCount(int totalCount, int pageSize)
    {
        return (totalCount + pageSize - 1) / pageSize;
    }

    public static void MockUpdateRealtimeProcessPageCount(int totalCount, int packetSize, out int pageSize,
        out int pageCount)
    {
        // sizeof(int)*5为4个数据包基本信息int字段+Processes数组长度int4个字节
        pageSize = (packetSize - SerializeHelper.PacketHeadLen - sizeof(int) * 5) /
                   RealtimeProcessItem.ObjectSize;
        pageCount = GetPageCount(totalCount, pageSize);
    }

    public static void MockUpdateGeneralProcessPageCount(int totalCount, int packetSize, out int pageSize,
        out int pageCount)
    {
        // sizeof(int)*5为4个数据包基本信息int字段+Processes数组长度int4个字节
        pageSize = (packetSize - SerializeHelper.PacketHeadLen - sizeof(int) * 5) /
                   GeneralProcessItem.ObjectSize;
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