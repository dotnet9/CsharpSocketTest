namespace SocketClient.Models;

/// <summary>
///     操作系统进程信息
/// </summary>
public class ProcessItemModel : BindableBase
{
    private string? _commandLine;

    private string? _cpu;
    private double _cpuValue;

    private string? _disk;
    private double _diskValue;

    private string? _gpu;
    private double _gpuValue;

    private string? _gpuEngine;

    private DateTime _lastUpdateTime;

    private string? _memory;
    private double _memoryValue;

    private string? _name;

    private string? _network;
    private double _networkValue;

    private string? _power;
    private ProcessPowerUsage _powerValue;

    private string? _powerUsageTrend;
    private ProcessPowerUsage _powerUsageTrendValue;

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
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    ///     进程类型
    /// </summary>
    public string? Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    /// <summary>
    ///     进程状态
    /// </summary>
    public string? Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    /// <summary>
    ///     发布者
    /// </summary>
    public string? Publisher
    {
        get => _publisher;
        set => SetProperty(ref _publisher, value);
    }

    /// <summary>
    ///     命令行
    /// </summary>
    public string? CommandLine
    {
        get => _commandLine;
        set => SetProperty(ref _commandLine, value);
    }

    /// <summary>
    ///     CPU使用率
    /// </summary>
    public string? CPU
    {
        get => _cpu;
        set => SetProperty(ref _cpu, value);
    }

    /// <summary>
    ///     CPU使用率
    /// </summary>
    public double CPUValue
    {
        get => _cpuValue;
        set => SetProperty(ref _cpuValue, value);
    }

    /// <summary>
    ///     内存使用大小
    /// </summary>
    public string? Memory
    {
        get => _memory;
        set => SetProperty(ref _memory, value);
    }

    /// <summary>
    ///     内存使用大小
    /// </summary>
    public double MemoryValue
    {
        get => _memoryValue;
        set => SetProperty(ref _memoryValue, value);
    }

    /// <summary>
    ///     磁盘使用大小
    /// </summary>
    public string? Disk
    {
        get => _disk;
        set => SetProperty(ref _disk, value);
    }

    /// <summary>
    ///     磁盘使用大小
    /// </summary>
    public double DiskValue
    {
        get => _diskValue;
        set => SetProperty(ref _diskValue, value);
    }

    /// <summary>
    ///     网络使用值
    /// </summary>
    public string? Network
    {
        get => _network;
        set => SetProperty(ref _network, value);
    }

    /// <summary>
    ///     网络使用值
    /// </summary>
    public double NetworkValue
    {
        get => _networkValue;
        set => SetProperty(ref _networkValue, value);
    }

    /// <summary>
    ///     GPU
    /// </summary>
    public string? GPU
    {
        get => _gpu;
        set => SetProperty(ref _gpu, value);
    }

    /// <summary>
    ///     GPU
    /// </summary>
    public double GPUValue
    {
        get => _gpuValue;
        set => SetProperty(ref _gpuValue, value);
    }

    /// <summary>
    ///     GPU引擎
    /// </summary>
    public string? GPUEngine
    {
        get => _gpuEngine;
        set => SetProperty(ref _gpuEngine, value);
    }

    /// <summary>
    ///     电源使用情况
    /// </summary>
    public string? Power
    {
        get => _power;
        set => SetProperty(ref _power, value);
    }

    /// <summary>
    ///     电源使用情况
    /// </summary>
    public ProcessPowerUsage PowerValue
    {
        get => _powerValue;
        set => SetProperty(ref _powerValue, value);
    }

    /// <summary>
    ///     电源使用情况趋势
    /// </summary>
    public string? PowerUsageTrend
    {
        get => _powerUsageTrend;
        set => SetProperty(ref _powerUsageTrend, value);
    }

    /// <summary>
    ///     电源使用情况
    /// </summary>
    public ProcessPowerUsage PowerUsageTrendValue
    {
        get => _powerUsageTrendValue;
        set => SetProperty(ref _powerUsageTrendValue, value);
    }

    /// <summary>
    ///     上次更新时间
    /// </summary>
    public DateTime LastUpdateTime
    {
        get => _lastUpdateTime;
        set => SetProperty(ref _lastUpdateTime, value);
    }

    /// <summary>
    ///     更新时间
    /// </summary>
    public DateTime UpdateTime
    {
        get => _updateTime;
        set => SetProperty(ref _updateTime, value);
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
        CPUValue = (process.ProcessData!.CPU * 1.0) / 10;
        CPU = CPUValue.ToString("P1");
        MemoryValue = (process.ProcessData!.Memory * 1.0) / 10;
        Memory = MemoryValue.ToString("P1");
        DiskValue = (process.ProcessData!.Disk * 1.0) / 10;
        Disk = $"{DiskValue:P1} MB/秒";
        NetworkValue = (process.ProcessData!.Network * 1.0) / 10;
        Network = $"{NetworkValue:P1} Mbps";
        GPUValue = (process.ProcessData!.GPU * 1.0) / 10;
        GPU = GPUValue.ToString("P1");
        GPUEngine = ((GpuEngine)Enum.Parse(typeof(GpuEngine), process.ProcessData!.GPUEngine.ToString())).Description();
        PowerValue =
            (ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), process.ProcessData!.PowerUsage.ToString());
        Power = PowerValue.Description();
        PowerUsageTrendValue =
            (ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), process.ProcessData!.PowerUsageTrend.ToString());
        PowerUsageTrend = PowerUsageTrendValue.Description();
        LastUpdateTime = process.LastUpdateTime.ToDateTime(timestampStartYear);
        UpdateTime = process.UpdateTime.ToDateTime(timestampStartYear);
    }

    public void Update(ActiveProcessItem process, byte timestampStartYear)
    {
        CPUValue = (process.ProcessData!.CPU * 1.0) / 10;
        CPU = CPUValue.ToString("P1");
        MemoryValue = (process.ProcessData!.Memory * 1.0) / 10;
        Memory = MemoryValue.ToString("P1");
        DiskValue = (process.ProcessData!.Disk * 1.0) / 10;
        Disk = $"{DiskValue:P1} MB/秒";
        NetworkValue = (process.ProcessData!.Network * 1.0) / 10;
        Network = $"{NetworkValue:P1} Mbps";
        GPUValue = (process.ProcessData!.GPU * 1.0) / 10;
        GPU = GPUValue.ToString("P1");
        GPUEngine = ((GpuEngine)Enum.Parse(typeof(GpuEngine), process.ProcessData!.GPUEngine.ToString())).Description();
        PowerValue =
            (ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), process.ProcessData!.PowerUsage.ToString());
        Power = PowerValue.Description();
        PowerUsageTrendValue =
            (ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), process.ProcessData!.PowerUsageTrend.ToString());
        PowerUsageTrend = PowerUsageTrendValue.Description();
        LastUpdateTime = UpdateTime;
        UpdateTime = process.UpdateTime.ToDateTime(timestampStartYear);
    }
}