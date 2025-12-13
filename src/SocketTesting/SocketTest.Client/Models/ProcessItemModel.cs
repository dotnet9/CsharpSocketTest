using CodeWF.Tools.Extensions;
using ReactiveUI;
using SocketDto.Enums;
using SocketDto.Response;
using System;

namespace SocketTest.Client.Models;

/// <summary>
///     操作系统进程信息
/// </summary>
public class ProcessItemModel : ReactiveObject
{
    public ProcessItemModel()
    {
    }

    public ProcessItemModel(ProcessItem process, int timestampStartYear)
    {
        Update(process, timestampStartYear);
    }

    /// <summary>
    ///     进程ID
    /// </summary>
    public int PID { get; set; }

    /// <summary>
    ///     进程名称
    /// </summary>
    public string? Name
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     进程类型
    /// </summary>
    public ProcessType Type
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     进程状态
    /// </summary>
    public ProcessStatus Status
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    private AlarmStatus _alarmStatus;

    /// <summary>
    /// 进程一般状态
    /// </summary>
    public AlarmStatus AlarmStatus
    {
        get => _alarmStatus;
        set => this.RaiseAndSetIfChanged(ref _alarmStatus, value);
    }

    /// <summary>
    ///     发布者
    /// </summary>
    public string? Publisher
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     命令行
    /// </summary>
    public string? CommandLine
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     CPU使用率
    /// </summary>
    public short Cpu
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     内存使用大小
    /// </summary>
    public short Memory
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     磁盘使用大小
    /// </summary>
    public short Disk
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     网络使用值
    /// </summary>
    public short Network
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     GPU
    /// </summary>
    public short Gpu
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     GPU引擎
    /// </summary>
    public GpuEngine GpuEngine
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     电源使用情况
    /// </summary>
    public PowerUsage PowerUsage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     电源使用情况趋势
    /// </summary>
    public PowerUsage PowerUsageTrend
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     上次更新时间
    /// </summary>
    public DateTime LastUpdateTime
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    ///     更新时间
    /// </summary>
    public DateTime UpdateTime
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public void Update(ProcessItem process, int timestampStartYear)
    {
        PID = process.Pid;

        Name = process.Name;
        Publisher = process.Publisher;
        CommandLine = process.CommandLine;

        Cpu = process.Cpu;
        Memory = process.Memory;
        Disk = process.Disk;
        Network = process.Network;
        Gpu = process.Gpu;
        GpuEngine = (GpuEngine)Enum.Parse(typeof(GpuEngine), process.GpuEngine.ToString());
        PowerUsage = (PowerUsage)Enum.Parse(typeof(PowerUsage), process.PowerUsage.ToString());
        PowerUsageTrend = (PowerUsage)Enum.Parse(typeof(PowerUsage), process.PowerUsageTrend.ToString());
        Type = (ProcessType)Enum.Parse(typeof(ProcessType), process.Type.ToString());
        Status = (ProcessStatus)Enum.Parse(typeof(ProcessStatus), process.ProcessStatus.ToString());
        LastUpdateTime = process.LastUpdateTime.FromSpecialUnixTimeSecondsToDateTime(timestampStartYear);
        UpdateTime = process.UpdateTime.FromSpecialUnixTimeSecondsToDateTime(timestampStartYear);
    }

    public void Update(short cpu, short memory, short disk, short network)
    {
        Cpu = cpu;
        Memory = memory;
        Disk = disk;
        Network = network;
    }

    public void Update(int timestampStartYear, byte processStatus, byte alarmStatus, short gpu, byte gpuEngine,
        byte powerUsage, byte powerUsageTrend, uint updateTime)
    {
        Status = (ProcessStatus)Enum.Parse(typeof(ProcessStatus), processStatus.ToString());
        AlarmStatus = (AlarmStatus)Enum.Parse(typeof(AlarmStatus), alarmStatus.ToString());
        Gpu = gpu;
        GpuEngine = (GpuEngine)Enum.Parse(typeof(GpuEngine), gpuEngine.ToString());
        PowerUsage = (PowerUsage)Enum.Parse(typeof(PowerUsage), powerUsage.ToString());
        PowerUsageTrend = (PowerUsage)Enum.Parse(typeof(PowerUsage), powerUsageTrend.ToString());
        LastUpdateTime = UpdateTime;
        UpdateTime = updateTime.FromSpecialUnixTimeSecondsToDateTime(timestampStartYear);
    }
}