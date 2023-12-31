﻿namespace SocketDto;

/// <summary>
/// 更新进程变化信息，序列化和反序列不能加压缩，部分双精度因为有效位数太长，可能导致UDP包过大而发送失败，所以UDP包不要加压缩
/// </summary>
[NetHead(6, 1)]
public class UpdateActiveProcessList : INetObject
{
	/// <summary>
	/// 总数据大小
	/// </summary>
	public int TotalSize { get; set; }

	/// <summary>
	/// 分页大小
	/// </summary>
	public int PageSize { get; set; }

	/// <summary>
	/// 总页数
	/// </summary>
	public int PageCount { get; set; }

	/// <summary>
	/// 页索引
	/// </summary>
	public int PageIndex { get; set; }

	/// <summary>
	/// 进程列表
	/// </summary>
	public List<ActiveProcessItem>? Processes { get; set; }
}

/// <summary>
/// 操作系统进程信息
/// </summary>
public class ActiveProcessItem
{
	/// <summary>
	/// 对象大小，Data为8字节，序列化时需要4字节表示byte[]长度，所有总大小为4+8+4=16
	/// </summary>
	public const int ObjectSize = 16;

	/// <summary>
	/// 见ActiveProcessData定义
	/// </summary>
	private byte[]? _data;

	/// <summary>
	/// 见ActiveProcessItemData
	/// </summary>
	public byte[]? Data
	{
		get => _data;
		set
		{
			_data = value;
			_processData = _data?.ToFieldObject<ActiveProcessItemData>();
		}
	}

	private ActiveProcessItemData? _processData;

	/// <summary>
	/// 进程数据
	/// </summary>
	[NetIgnoreMember]
	public ActiveProcessItemData? ProcessData
	{
		get => _processData;
		set
		{
			_processData = value;
			_data = _processData?.FieldObjectBuffer();
		}
	}

	/// <summary>
	/// 更新时间（当天时间戳：当日0点0分0秒计算的时间戳，单位ms）
	/// </summary>
	public uint UpdateTime { get; set; }
}

public record ActiveProcessItemData
{
	/// <summary>
	/// 占10bit, CPU（所有内核的总处理利用率），最后一位表示小数位，比如253表示25.3%
	/// </summary>
	[NetFieldOffset(0, 10)]
	public short CPU { get; set; }

	/// <summary>
	/// 占10bit, 内存（进程占用的物理内存），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
	/// </summary>
	[NetFieldOffset(10, 10)]
	public short Memory { get; set; }

	/// <summary>
	/// 占10bit, 磁盘（所有物理驱动器的总利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
	/// </summary>
	[NetFieldOffset(20, 10)]
	public short Disk { get; set; }

	/// <summary>
	/// 占10bit, 网络（当前主要网络上的网络利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
	/// </summary>
	[NetFieldOffset(30, 10)]
	public short Network { get; set; }

	/// <summary>
	/// 占10bit, GPU(所有GPU引擎的最高利用率)，最后一位表示小数位，比如253表示25.3
	/// </summary>
	[NetFieldOffset(40, 10)]
	public short GPU { get; set; }

	/// <summary>
	/// 占1bit，GPU引擎，0：无，1：GPU 0 - 3D
	/// </summary>
	[NetFieldOffset(50, 1)]
	public byte GPUEngine { get; set; }

	/// <summary>
	/// 占3bit，电源使用情况（CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
	/// </summary>
	[NetFieldOffset(51, 3)]
	public byte PowerUsage { get; set; }

	/// <summary>
	/// 占3bit，电源使用情况趋势（一段时间内CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
	/// </summary>
	[NetFieldOffset(54, 3)]
	public byte PowerUsageTrend { get; set; }

	public override string ToString()
	{
		return
			$"{nameof(CPU)}={CPU}，{nameof(Memory)}={Memory}，{nameof(Disk)}={Disk}，{nameof(Network)}={Network}，{nameof(GPU)}={GPU}，{nameof(GPUEngine)}={GPUEngine}，{nameof(PowerUsage)}={PowerUsage}，{nameof(PowerUsageTrend)}={PowerUsageTrend}";
	}
}