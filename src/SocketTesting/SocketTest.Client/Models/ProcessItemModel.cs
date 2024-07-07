using System;
using System.Collections;
using CodeWF.Tools.Extensions;
using ReactiveUI;
using SocketDto.Enums;
using SocketDto.Response;
using SocketTest.Common;
using SocketTest.Mvvm;

namespace SocketTest.Client.Models;

/// <summary>
///     操作系统进程信息
/// </summary>
public class ProcessItemModel : ViewModelBase
{
    private string? _commandLine;

    private short _cpu;

    private short _disk;

    private short _gpu;

    private GpuEngine _gpuEngine;

    private DateTime _lastUpdateTime;

    private short _memory;

    private string? _name;

    private short _network;

    private PowerUsage _powerUsage;

    private PowerUsage _powerUsageTrend;

    private string? _publisher;

    private ProcessStatus _status;

    private ProcessType _type;

    private DateTime _updateTime;

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
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    ///     进程类型
    /// </summary>
    public ProcessType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    ///     进程状态
    /// </summary>
    public ProcessStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
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
        get => _publisher;
        set => this.RaiseAndSetIfChanged(ref _publisher, value);
    }

    /// <summary>
    ///     命令行
    /// </summary>
    public string? CommandLine
    {
        get => _commandLine;
        set => this.RaiseAndSetIfChanged(ref _commandLine, value);
    }

    /// <summary>
    ///     CPU使用率
    /// </summary>
    public short Cpu
    {
        get => _cpu;
        set => this.RaiseAndSetIfChanged(ref _cpu, value);
    }

    /// <summary>
    ///     内存使用大小
    /// </summary>
    public short Memory
    {
        get => _memory;
        set => this.RaiseAndSetIfChanged(ref _memory, value);
    }

    /// <summary>
    ///     磁盘使用大小
    /// </summary>
    public short Disk
    {
        get => _disk;
        set => this.RaiseAndSetIfChanged(ref _disk, value);
    }

    /// <summary>
    ///     网络使用值
    /// </summary>
    public short Network
    {
        get => _network;
        set => this.RaiseAndSetIfChanged(ref _network, value);
    }

    /// <summary>
    ///     GPU
    /// </summary>
    public short Gpu
    {
        get => _gpu;
        set => this.RaiseAndSetIfChanged(ref _gpu, value);
    }

    /// <summary>
    ///     GPU引擎
    /// </summary>
    public GpuEngine GpuEngine
    {
        get => _gpuEngine;
        set => this.RaiseAndSetIfChanged(ref _gpuEngine, value);
    }

    /// <summary>
    ///     电源使用情况
    /// </summary>
    public PowerUsage PowerUsage
    {
        get => _powerUsage;
        set => this.RaiseAndSetIfChanged(ref _powerUsage, value);
    }

    /// <summary>
    ///     电源使用情况趋势
    /// </summary>
    public PowerUsage PowerUsageTrend
    {
        get => _powerUsageTrend;
        set => this.RaiseAndSetIfChanged(ref _powerUsageTrend, value);
    }

    /// <summary>
    ///     上次更新时间
    /// </summary>
    public DateTime LastUpdateTime
    {
        get => _lastUpdateTime;
        set => this.RaiseAndSetIfChanged(ref _lastUpdateTime, value);
    }

    /// <summary>
    ///     更新时间
    /// </summary>
    public DateTime UpdateTime
    {
        get => _updateTime;
        set => this.RaiseAndSetIfChanged(ref _updateTime, value);
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