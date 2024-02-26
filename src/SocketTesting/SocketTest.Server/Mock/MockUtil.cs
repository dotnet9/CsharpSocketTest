using LoremNET;
using SocketDto.Enums;
using SocketDto.Response;
using SocketDto.Udp;
using SocketNetObject;
using SocketTest.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ProcessItem = SocketDto.Response.ProcessItem;

namespace SocketTest.Server.Mock;

public static class MockUtil
{
    public const byte TimestampStartYear = 23;
    private static int _mockCount;
    private static ResponseServiceInfo? _mockResponseBase;
    private static int[]? _mockProcessIdList;
    private static List<ProcessItem>? _mockProcesses;

    private static readonly Random CustomRandom = new(DateTime.Now.Microsecond);
    public static bool IsInitOver { get; private set; }

    public static async Task MockAsync(int total)
    {
        _mockCount = total;
        var stopwatch = new Stopwatch();

        stopwatch.Restart();
        await MockBaseAsync();
        stopwatch.Stop();
        Logger.Logger.Info($"模拟基本信息：{stopwatch.ElapsedMilliseconds}ms");

        stopwatch.Restart();
        await MockProcessIdListAsync();
        stopwatch.Stop();
        Logger.Logger.Info($"模拟 {total} 进程ID列表：{stopwatch.ElapsedMilliseconds}ms");

        stopwatch.Restart();
        await MockProcessAsync();
        stopwatch.Stop();
        Logger.Logger.Info($"模拟 {total} 进程详细信息列表：{stopwatch.ElapsedMilliseconds}ms");

        stopwatch.Restart();
        MockCreateUpdateData();
        stopwatch.Stop();
        Logger.Logger.Info($"模拟 {total} 进程实时更新yte[]：{stopwatch.ElapsedMilliseconds}ms");
        IsInitOver = true;
    }

    public static async Task<ResponseServiceInfo?> GetBaseInfoAsync()
    {
        return await Task.FromResult(_mockResponseBase);
    }

    public static async Task<int[]?> GetProcessIdListAsync()
    {
        return await Task.FromResult(_mockProcessIdList);
    }

    private static async Task MockBaseAsync()
    {
        _mockResponseBase = new ResponseServiceInfo
        {
            OS = "Windows 11 专业版",
            MemorySize = 64,
            ProcessorCount = 12,
            DiskSize = 2048,
            NetworkBandwidth = 1024,
            Ips = "192.32.35.23",
            TimestampStartYear = TimestampStartYear,
            LastUpdateTime = TimestampStartYear.GetCurrentTimestamp()
        };
        await Task.CompletedTask;
    }

    private static async Task MockProcessIdListAsync()
    {
        _mockProcessIdList = Enumerable.Range(1, _mockCount).Select((index, value) => index).ToArray();
        await Task.CompletedTask;
    }


    private static async Task<bool> MockProcessAsync()
    {
        var mockStr = Lorem.Words(1, 3);
        var mockShort = (short)Random.Shared.Next(0, 1000);
        var timestamp = TimestampStartYear.GetCurrentTimestamp();

        ProcessItem MockProcess(int id)
        {
            return new ProcessItem
            {
                Pid = id + 1,
                Name = mockStr,
                Type = (byte)ProcessType.Application,
                ProcessStatus = (byte)ProcessStatus.Ready,
                AlarmStatus = (byte)AlarmStatus.Normal,
                Publisher = mockStr,
                CommandLine = mockStr,
                Cpu = mockShort,
                Memory = mockShort,
                Disk = mockShort,
                Network = mockShort,
                Gpu = mockShort,
                GpuEngine = (byte)GpuEngine.Gpu03D,
                PowerUsage = (byte)PowerUsage.Low,
                PowerUsageTrend = (byte)PowerUsage.Low,
                LastUpdateTime = timestamp,
                UpdateTime = timestamp
            };
        }

        _mockProcesses = Enumerable.Range(0, _mockCount).Select(MockProcess).ToList();
        return await Task.FromResult(true);
    }

    public static async Task<List<ProcessItem>> MockProcessesAsync(int pageSize, int pageIndex)
    {
        return await Task.FromResult(_mockProcesses!.Skip(pageIndex * pageSize).Take(pageSize).ToList());
    }


    private static byte[]? _mockCpus;
    private static byte[]? _mockMemories;
    private static byte[]? _mockDisks;
    private static byte[]? _mockNetworks;
    private static byte[]? _mockProcessStatuses;
    private static byte[]? _mockAlarmStatuses;
    private static byte[]? _mockGpus;
    private static byte[]? _mockGpuEngines;
    private static byte[]? _mockPowerUsages;
    private static byte[]? _mockPowerUsageTrends;
    private static byte[]? _mockUpdateTimes;

    public static void MockCreateUpdateData()
    {
        _mockCpus = new byte[_mockCount * 2];
        _mockMemories = new byte[_mockCount * 2];
        _mockDisks = new byte[_mockCount * 2];
        _mockNetworks = new byte[_mockCount * 2];
        _mockProcessStatuses = new byte[_mockCount * 1];
        _mockAlarmStatuses = new byte[_mockCount * 1];
        _mockGpus = new byte[_mockCount * 2];
        _mockGpuEngines = new byte[_mockCount * 1];
        _mockPowerUsages = new byte[_mockCount * 1];
        _mockPowerUsageTrends = new byte[_mockCount * 1];
        _mockUpdateTimes = new byte[_mockCount * 4];
    }

    public static async Task MockUpdateDataAsync()
    {
        var mockShort = (short)Random.Shared.Next(0, 1000);
        var mockProcessStatus = (byte)Random.Shared.Next(0, 5);
        var mockAlarmStatus = (byte)Random.Shared.Next(0, 8);
        var mockGpuEngine = (byte)Random.Shared.Next(0, 2);
        var mockPowerUsage = (byte)Random.Shared.Next(0, 5);
        var mockPowerUsageTrend = (byte)Random.Shared.Next(0, 5);
        var timestamp = TimestampStartYear.GetCurrentTimestamp();

        Update(_mockCpus!, mockShort);
        Update(_mockMemories!, mockShort);
        Update(_mockDisks!, mockShort);
        Update(_mockNetworks!, mockShort);
        Update(_mockProcessStatuses!, mockProcessStatus);
        Update(_mockAlarmStatuses!, mockAlarmStatus);
        Update(_mockGpus!, mockShort);
        Update(_mockGpuEngines!, mockGpuEngine);
        Update(_mockPowerUsages!, mockPowerUsage);
        Update(_mockPowerUsageTrends!, mockPowerUsageTrend);
        Update(_mockUpdateTimes!, timestamp);

        await Task.CompletedTask;
    }

    public static UpdateRealtimeProcessList MockUpdateRealtimeProcessList(int pageSize, int pageIndex)
    {
        var dataCount = GetDataCount(_mockCount, pageSize, pageIndex);
        var data = new UpdateRealtimeProcessList
        {
            Cpus = new byte[dataCount * 2],
            Memories = new byte[dataCount * 2],
            Disks = new byte[dataCount * 2],
            Networks = new byte[dataCount * 2]
        };
        Buffer.BlockCopy(_mockCpus!, (pageIndex * pageSize * 2), data.Cpus, 0, data.Cpus.Length);
        Buffer.BlockCopy(_mockMemories!, (pageIndex * pageSize * 2), data.Memories, 0, data.Memories.Length);
        Buffer.BlockCopy(_mockDisks!, (pageIndex * pageSize * 2), data.Disks, 0, data.Disks.Length);
        Buffer.BlockCopy(_mockNetworks!, (pageIndex * pageSize * 2), data.Networks, 0, data.Networks.Length);

        return data;
    }

    public static UpdateGeneralProcessList MockUpdateGeneralProcessList(int pageSize, int pageIndex)
    {
        var dataCount = GetDataCount(_mockCount, pageSize, pageIndex);
        var data = new UpdateGeneralProcessList()
        {
            ProcessStatuses = new byte[dataCount * 1],
            AlarmStatuses = new byte[dataCount * 1],
            Gpus = new byte[dataCount * 2],
            GpuEngine = new byte[dataCount * 1],
            PowerUsage = new byte[dataCount * 1],
            PowerUsageTrend = new byte[dataCount * 1],
            UpdateTimes = new byte[dataCount * 4]
        };
        Buffer.BlockCopy(_mockProcessStatuses!, (pageIndex * pageSize * 1), data.ProcessStatuses, 0,
            data.ProcessStatuses.Length);
        Buffer.BlockCopy(_mockAlarmStatuses!, (pageIndex * pageSize * 1), data.AlarmStatuses, 0,
            data.AlarmStatuses.Length);
        Buffer.BlockCopy(_mockGpus!, (pageIndex * pageSize * 2), data.Gpus, 0, data.Gpus.Length);
        Buffer.BlockCopy(_mockGpuEngines!, (pageIndex * pageSize * 1), data.GpuEngine, 0, data.GpuEngine.Length);
        Buffer.BlockCopy(_mockPowerUsages!, (pageIndex * pageSize * 1), data.PowerUsage, 0, data.PowerUsage.Length);
        Buffer.BlockCopy(_mockPowerUsageTrends!, (pageIndex * pageSize * 1), data.PowerUsageTrend, 0,
            data.PowerUsageTrend.Length);
        Buffer.BlockCopy(_mockUpdateTimes!, (pageIndex * pageSize * 4), data.UpdateTimes, 0, data.UpdateTimes.Length);

        return data;
    }

    public static void MockUpdateRealtimeProcessPageCount(int totalCount, int packetSize, out int pageSize,
        out int pageCount)
    {
        // (UDP单包大小 - 数据包头部大小 - 4个int字段大小 - 4个byte[]数组长度占位大小) / 4 个byte[]单个进程占8个字节
        pageSize = (packetSize - SerializeHelper.PacketHeadLen - sizeof(int) * 4 - sizeof(int) * 4) /
                   (sizeof(short) * 4);
        pageCount = GetPageCount(totalCount, pageSize);
    }

    public static void MockUpdateGeneralProcessPageCount(int totalCount, int packetSize, out int pageSize,
        out int pageCount)
    {
        // (UDP单包大小 - 数据包头部大小 - 4个int字段大小 - 7个byte[]数组长度占位大小) / 7 个byte[]单个进程占11个字节
        pageSize = (packetSize - SerializeHelper.PacketHeadLen - sizeof(int) * 4 - sizeof(int) * 7) /
                   (sizeof(byte) * 5 + sizeof(short) + sizeof(uint));
        pageCount = GetPageCount(totalCount, pageSize);
    }

    static unsafe void Update(byte[] array, byte value)
    {
        fixed (byte* pArray = array)
        {
            var pByte = pArray;
            for (var i = 0; i < _mockCount; i++)
            {
                *pByte = value;
                pByte++;
            }
        }
    }

    static unsafe void Update(byte[] array, short value)
    {
        fixed (byte* pArray = array)
        {
            var pByte = (short*)pArray;
            for (var i = 0; i < _mockCount; i++)
            {
                *pByte = value;
                pByte++;
            }
        }
    }

    static unsafe void Update(byte[] array, uint value)
    {
        fixed (byte* pArray = array)
        {
            var pByte = (uint*)pArray;
            for (var i = 0; i < _mockCount; i++)
            {
                *pByte = value;
                pByte++;
            }
        }
    }


    public static int GetPageCount(int totalCount, int pageSize)
    {
        return (totalCount + pageSize - 1) / pageSize;
    }

    public static int GetDataCount(int totalCount, int pageSize, int pageIndex)
    {
        var dataIndex = pageIndex * pageSize;
        var dataCount = totalCount - dataIndex;
        if (dataCount > pageSize) dataCount = pageSize;

        return dataCount;
    }
}

public enum MockDataType
{
    Cpu,
    Memory,
    Dis,
    Network
}