namespace SocketClient.Models;

/// <summary>
///     操作系统进程信息
/// </summary>
public class ProcessItemModel : BindableBase
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
	public short CPU
	{
		get => _cpu;
		set => SetProperty(ref _cpu, value);
	}

	/// <summary>
	///     内存使用大小
	/// </summary>
	public short Memory
	{
		get => _memory;
		set => SetProperty(ref _memory, value);
	}

	/// <summary>
	///     磁盘使用大小
	/// </summary>
	public short Disk
	{
		get => _disk;
		set => SetProperty(ref _disk, value);
	}

	/// <summary>
	///     网络使用值
	/// </summary>
	public short Network
	{
		get => _network;
		set => SetProperty(ref _network, value);
	}

	/// <summary>
	///     GPU
	/// </summary>
	public short GPU
	{
		get => _gpu;
		set => SetProperty(ref _gpu, value);
	}

	/// <summary>
	///     GPU引擎
	/// </summary>
	public byte GPUEngine
	{
		get => _gpuEngine;
		set => SetProperty(ref _gpuEngine, value);
	}

	/// <summary>
	///     电源使用情况
	/// </summary>
	public byte Power
	{
		get => _power;
		set => SetProperty(ref _power, value);
	}

	/// <summary>
	///     电源使用情况趋势
	/// </summary>
	public byte PowerUsageTrend
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