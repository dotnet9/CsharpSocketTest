using Process = SocketDto.Process;

namespace SocketClient.Models;

/// <summary>
///     操作系统进程信息
/// </summary>
public class ProcessItem : BindableBase
{
    private string? _commandLine;

    private string? _cpuUsage;

    private string? _diskUsage;

    private string? _gpu;

    private string? _gpuEngine;

    private DateTime _lastUpdateTime;


    private DateTime _lastViewUpdateTime;

    private string? _memoryUsage;

    private string? _name;

    private string? _networkUsage;

    private string? _powerUsage;

    private string? _powerUsageTrend;

    private string? _publisher;

    private string? _status;

    private string? _type;

    private DateTime _updateTime;

    private DateTime _viewUpdateTime;

    public ProcessItem()
    {
    }

    public ProcessItem(Process process)
    {
        Update(process);
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
    public string? CPUUsage
    {
        get => _cpuUsage;
        set => SetProperty(ref _cpuUsage, value);
    }

    /// <summary>
    ///     内存使用大小
    /// </summary>
    public string? MemoryUsage
    {
        get => _memoryUsage;
        set => SetProperty(ref _memoryUsage, value);
    }

    /// <summary>
    ///     磁盘使用大小
    /// </summary>
    public string? DiskUsage
    {
        get => _diskUsage;
        set => SetProperty(ref _diskUsage, value);
    }

    /// <summary>
    ///     网络使用值
    /// </summary>
    public string? NetworkUsage
    {
        get => _networkUsage;
        set => SetProperty(ref _networkUsage, value);
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
    public string? PowerUsage
    {
        get => _powerUsage;
        set => SetProperty(ref _powerUsage, value);
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

    /// <summary>
    ///     上次视图更新时间
    /// </summary>
    public DateTime LastViewUpdateTime
    {
        get => _lastViewUpdateTime;
        set
        {
            if (value != _lastViewUpdateTime) SetProperty(ref _lastViewUpdateTime, value);
        }
    }

    /// <summary>
    ///     视图更新时间
    /// </summary>
    public DateTime ViewUpdateTime
    {
        get => _viewUpdateTime;
        set
        {
            if (value != _viewUpdateTime) SetProperty(ref _viewUpdateTime, value);
        }
    }

    public void Update(Process process)
    {
        PID = process.PID;

        Name = process.Name;
        Type = ((ProcessType)Enum.Parse(typeof(ProcessType), process.Type.ToString())).Description();
        Status = ((ProcessStatus)Enum.Parse(typeof(ProcessStatus), process.Status.ToString())).Description();
        Publisher = process.Publisher;
        CommandLine = process.CommandLine;

        CPUUsage = process.CPUUsage.ToString("P1");
        MemoryUsage = process.MemoryUsage.ToString("P1");
        DiskUsage = $"{process.DiskUsage:P1} MB/秒";
        NetworkUsage = $"{process.DiskUsage:P1} Mbps";
        GPU = process.GPU.ToString("P1");
        GPUEngine = process.GPUEngine;
        PowerUsage = ((ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), process.PowerUsage.ToString()))
            .Description();
        PowerUsageTrend = ((ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), process.PowerUsageTrend.ToString()))
            .Description();
        LastUpdateTime = process.LastUpdateTime.ToDateTime();
        UpdateTime = process.UpdateTime.ToDateTime();
    }

    public void Update(ActiveProcess process)
    {
        PID = process.PID;

        CPUUsage = process.CPUUsage.ToString("P1");
        MemoryUsage = process.MemoryUsage.ToString("P1");
        DiskUsage = $"{process.DiskUsage:P1} MB/秒";
        NetworkUsage = $"{process.DiskUsage:P1} Mbps";
        GPU = process.GPU.ToString("P1");
        PowerUsage = ((ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), process.PowerUsage.ToString()))
            .Description();
        PowerUsageTrend = ((ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), process.PowerUsageTrend.ToString()))
            .Description();
        LastUpdateTime = UpdateTime;
        UpdateTime = process.UpdateTime.ToDateTime();


        LastViewUpdateTime = ViewUpdateTime;
        ViewUpdateTime = DateTime.Now;
    }
}