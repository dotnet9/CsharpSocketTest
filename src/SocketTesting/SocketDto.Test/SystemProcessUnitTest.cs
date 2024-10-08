using CodeWF.NetWeaver;
using CodeWF.NetWeaver.Base;
using System.Text;
using System.Text.Json;
using Xunit.Abstractions;

namespace SocketDto.Test;

public class SystemProcessUnitTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    /// <summary>
    ///     未优化
    /// </summary>
    private readonly SystemProcess _codeWFObject = new()
    {
        PID = 10565,
        Name = "码界工坊",
        Publisher = "沙漠尽头的狼",
        CommandLine = "dotnet CodeWF.Tools.dll",
        CPU = "2.3%",
        Memory = "0.1%",
        Disk = "0.1 MB/秒",
        Network = "0 Mbps",
        GPU = "2.2%",
        GPUEngine = "GPU 0 - 3D",
        PowerUsage = "低",
        PowerUsageTrend = "非常低",
        Type = "应用",
        Status = "效率模式"
    };

    /// <summary>
    ///     普通优化字段数据类型
    /// </summary>
    private readonly SystemProcess2 _codeWFObject2 = new()
    {
        PID = 10565,
        Name = "码界工坊",
        Publisher = "沙漠尽头的狼",
        CommandLine = "dotnet CodeWF.Tools.dll",
        CPU = 2.3f,
        Memory = 0.1f,
        Disk = 0.1f,
        Network = 0,
        GPU = 2.2f,
        GPUEngine = 1,
        PowerUsage = 1,
        PowerUsageTrend = 0,
        Type = 0,
        Status = 1
    };

    /// <summary>
    ///     极限优化字段数据类型
    /// </summary>
    private readonly SystemProcess3 _codeWFObject3 = new()
    {
        PID = 10565,
        Name = "码界工坊",
        Publisher = "沙漠尽头的狼",
        CommandLine = "dotnet CodeWF.Tools.dll",
        ProcessData = new SystemProcessData
        {
            CPU = 23,
            Memory = 1,
            Disk = 1,
            Network = 0,
            GPU = 22,
            GPUEngine = 1,
            PowerUsage = 1,
            PowerUsageTrend = 0,
            Type = 0,
            Status = 1
        }
    };

    public SystemProcessUnitTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    ///     二进制极限序列化测试
    /// </summary>
    [Fact]
    public void Test_SerializeToBytes3_Success()
    {
        var buffer = _codeWFObject3.Serialize(1);
        _testOutputHelper.WriteLine($"序列化后二进制长度：{buffer.Length}");

        var deserializeObj = buffer.Deserialize<SystemProcess3>();
        Assert.Equal("码界工坊", deserializeObj.Name);
        Assert.Equal(23, deserializeObj.ProcessData!.CPU);
        Assert.Equal(1, deserializeObj.ProcessData.PowerUsage);
    }
}

[NetHead(1, 1)]
public class SystemProcess : INetObject
{
    public int PID { get; set; }
    public string? Name { get; set; }
    public string? Publisher { get; set; }
    public string? CommandLine { get; set; }
    public string? CPU { get; set; }
    public string? Memory { get; set; }
    public string? Disk { get; set; }
    public string? Network { get; set; }
    public string? GPU { get; set; }
    public string? GPUEngine { get; set; }
    public string? PowerUsage { get; set; }
    public string? PowerUsageTrend { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
}

[NetHead(1, 2)]
public class SystemProcess2 : INetObject
{
    public int PID { get; set; }
    public string? Name { get; set; }
    public string? Publisher { get; set; }
    public string? CommandLine { get; set; }
    public float CPU { get; set; }
    public float Memory { get; set; }
    public float Disk { get; set; }
    public float Network { get; set; }
    public float GPU { get; set; }
    public byte GPUEngine { get; set; }
    public byte PowerUsage { get; set; }
    public byte PowerUsageTrend { get; set; }
    public byte Type { get; set; }
    public byte Status { get; set; }
}

[NetHead(1, 3)]
public class SystemProcess3 : INetObject
{
    private byte[]? _data;

    private SystemProcessData? _processData;
    public int PID { get; set; }
    public string? Name { get; set; }
    public string? Publisher { get; set; }
    public string? CommandLine { get; set; }

    /// <summary>
    ///     序列化，这是实际需要序列化的数据
    /// </summary>
    public byte[]? Data
    {
        get => _data;
        set
        {
            _data = value;

            // 这是关键：在反序列化将byte转换为对象，方便程序中使用
            _processData = _data?.ToFieldObject<SystemProcessData>();
        }
    }

    /// <summary>
    ///     进程数据，添加NetIgnoreMember在序列化会忽略
    /// </summary>
    [NetIgnoreMember]
    public SystemProcessData? ProcessData
    {
        get => _processData;
        set
        {
            _processData = value;

            // 这里关键：将对象转换为位域
            _data = _processData?.FieldObjectBuffer();
        }
    }
}

public record SystemProcessData
{
    [NetFieldOffset(0, 10)] public short CPU { get; set; }
    [NetFieldOffset(10, 10)] public short Memory { get; set; }
    [NetFieldOffset(20, 10)] public short Disk { get; set; }
    [NetFieldOffset(30, 10)] public short Network { get; set; }
    [NetFieldOffset(40, 10)] public short GPU { get; set; }
    [NetFieldOffset(50, 1)] public byte GPUEngine { get; set; }
    [NetFieldOffset(51, 3)] public byte PowerUsage { get; set; }
    [NetFieldOffset(54, 3)] public byte PowerUsageTrend { get; set; }
    [NetFieldOffset(57, 1)] public byte Type { get; set; }
    [NetFieldOffset(58, 2)] public byte Status { get; set; }
}