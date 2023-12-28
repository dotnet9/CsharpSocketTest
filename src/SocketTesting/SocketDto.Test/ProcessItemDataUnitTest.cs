namespace SocketDto.Test;

public class ProcessItemDataUnitTest
{
    [Fact]
    public void Test_SerializeProcessItemData_Success()
    {
        var data = new ProcessItemData
        {
            Cpu = 112,
            Memory = 325,
            Disk = 23,
            Network = 593,
            Gpu = 253,
            GpuEngine = (byte)GpuEngine.None,
            PowerUsage = (byte)ProcessPowerUsage.Low,
            PowerUsageTrend = (byte)ProcessPowerUsage.Low,
            Type = (byte)ProcessType.Application,
            Status = (byte)ProcessStatus.Running
        };

        var buffer = data.FieldObjectBuffer();
        var desData = buffer.ToFieldObject<ProcessItemData>();

        // ProcessItemData总共60位，序列化后应该占8个字节
        Assert.Equal(8, buffer.Length);
        Assert.Equal(data.Cpu, desData.Cpu);
        Assert.Equal(data.Status, desData.Status);
    }
}