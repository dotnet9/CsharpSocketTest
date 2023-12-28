namespace SocketDto.Test;

public class ActiveProcessItemDataUnitTest
{
    [Fact]
    public void Test_SerializeActiveProcessItemData_Success()
    {
        var data = new ActiveProcessItemData
        {
            Cpu = 112,
            Memory = 325,
            Disk = 23,
            Network = 593,
            Gpu = 253,
            GpuEngine = (byte)GpuEngine.None,
            PowerUsage = (byte)ProcessPowerUsage.Low,
            PowerUsageTrend = (byte)ProcessPowerUsage.Low
        };

        var buffer = data.FieldObjectBuffer();
        var desData = buffer.ToFieldObject<ActiveProcessItemData>();

        // ActiveProcessItemData�ܹ�57λ�����л���Ӧ��ռ8���ֽ�
        Assert.Equal(8, buffer.Length);
        Assert.Equal(data.Cpu, desData.Cpu);
    }
}