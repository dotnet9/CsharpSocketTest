using System;
using ReactiveUI;
using SocketDto;
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

    private byte _gpuEngine;

    private DateTime _lastUpdateTime;

    private short _memory;

    private string? _name;

    private short _network;

    private byte _power;

    private byte _powerUsageTrend;

    private string? _publisher;

    private string? _status;

    private string? _type;

    private DateTime _updateTime;

    public ProcessItemModel()
    {
    }

    public ProcessItemModel(ProcessItem process, byte timestampStartYear)
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
    public string? Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    ///     进程状态
    /// </summary>
    public string? Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
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
    public short CPU
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
    public short GPU
    {
        get => _gpu;
        set => this.RaiseAndSetIfChanged(ref _gpu, value);
    }

    /// <summary>
    ///     GPU引擎
    /// </summary>
    public byte GPUEngine
    {
        get => _gpuEngine;
        set => this.RaiseAndSetIfChanged(ref _gpuEngine, value);
    }

    /// <summary>
    ///     电源使用情况
    /// </summary>
    public byte Power
    {
        get => _power;
        set => this.RaiseAndSetIfChanged(ref _power, value);
    }

    /// <summary>
    ///     电源使用情况趋势
    /// </summary>
    public byte PowerUsageTrend
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

    public void Update(ProcessItem process, byte timestampStartYear)
    {
        PID = process.PID;

        Name = process.Name;
        Publisher = process.Publisher;
        CommandLine = process.CommandLine;

        Type = ((ProcessType)Enum.Parse(typeof(ProcessType), process.ProcessData!.Type.ToString())).Description();
        Status =
            ((ProcessStatus)Enum.Parse(typeof(ProcessStatus), process.ProcessData!.Status.ToString())).Description();
        CPU = process.ProcessData!.CPU;
        Memory = process.ProcessData!.Memory;
        Disk = process.ProcessData!.Disk;
        Network = process.ProcessData!.Network;
        GPU = process.ProcessData!.GPU;
        GPUEngine = process.ProcessData!.GPUEngine;
        Power = process.ProcessData!.PowerUsage;
        PowerUsageTrend = process.ProcessData!.PowerUsageTrend;
        LastUpdateTime = process.LastUpdateTime.ToDateTime(timestampStartYear);
        UpdateTime = process.UpdateTime.ToDateTime(timestampStartYear);
    }

    public void Update(ActiveProcessItem process, byte timestampStartYear)
    {
        CPU = process.ProcessData!.CPU;
        Memory = process.ProcessData!.Memory;
        Disk = process.ProcessData!.Disk;
        Network = process.ProcessData!.Network;
        GPU = process.ProcessData!.GPU;
        GPUEngine = process.ProcessData!.GPUEngine;
        Power = process.ProcessData!.PowerUsage;
        PowerUsageTrend = process.ProcessData!.PowerUsageTrend;
        LastUpdateTime = UpdateTime;
        UpdateTime = process.UpdateTime.ToDateTime(timestampStartYear);
    }
}