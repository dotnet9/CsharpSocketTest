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
    ///     δ�Ż�
    /// </summary>
    private readonly SystemProcess _codeWFObject = new()
    {
        PID = 10565,
        Name = "��繤��",
        Publisher = "ɳĮ��ͷ����",
        CommandLine = "dotnet CodeWF.Tools.dll",
        CPU = "2.3%",
        Memory = "0.1%",
        Disk = "0.1 MB/��",
        Network = "0 Mbps",
        GPU = "2.2%",
        GPUEngine = "GPU 0 - 3D",
        PowerUsage = "��",
        PowerUsageTrend = "�ǳ���",
        Type = "Ӧ��",
        Status = "Ч��ģʽ"
    };

    /// <summary>
    ///     ��ͨ�Ż��ֶ���������
    /// </summary>
    private readonly SystemProcess2 _codeWFObject2 = new()
    {
        PID = 10565,
        Name = "��繤��",
        Publisher = "ɳĮ��ͷ����",
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
    ///     �����Ż��ֶ���������
    /// </summary>
    private readonly SystemProcess3 _codeWFObject3 = new()
    {
        PID = 10565,
        Name = "��繤��",
        Publisher = "ɳĮ��ͷ����",
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
    ///     �����Ƽ������л�����
    /// </summary>
    [Fact]
    public void Test_SerializeToBytes3_Success()
    {
        var buffer = _codeWFObject3.Serialize(1);
        _testOutputHelper.WriteLine($"���л�������Ƴ��ȣ�{buffer.Length}");

        var deserializeObj = buffer.Deserialize<SystemProcess3>();
        Assert.Equal("��繤��", deserializeObj.Name);
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
    ///     ���л�������ʵ����Ҫ���л�������
    /// </summary>
    public byte[]? Data
    {
        get => _data;
        set
        {
            _data = value;

            // ���ǹؼ����ڷ����л���byteת��Ϊ���󣬷��������ʹ��
            _processData = _data?.ToFieldObject<SystemProcessData>();
        }
    }

    /// <summary>
    ///     �������ݣ�����NetIgnoreMember�����л������
    /// </summary>
    [NetIgnoreMember]
    public SystemProcessData? ProcessData
    {
        get => _processData;
        set
        {
            _processData = value;

            // ����ؼ���������ת��Ϊλ��
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