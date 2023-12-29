namespace SocketDto.Test;

public class UpdateActiveProcessListUnitTest
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

        // ActiveProcessItemData总共57位，序列化后应该占8个字节
        Assert.Equal(8, buffer.Length);
        Assert.Equal(data.Cpu, desData.Cpu);
    }


    [Fact]
    public void Test_SerializeUpdateActiveProcessList_Success()
    {
        var netObject = new UpdateActiveProcessList()
        {
            TotalSize = 200,
            PageSize = 3,
            PageCount = 67,
            PageIndex = 1,
            Processes = new List<ActiveProcessItem>()
        };
        var processItem = new ActiveProcessItem
        {
            ProcessData = new ActiveProcessItemData()
            {
                Cpu = 112,
                Memory = 325,
                Disk = 23,
                Network = 593,
                Gpu = 253,
                GpuEngine = (byte)GpuEngine.None,
                PowerUsage = (byte)ProcessPowerUsage.Low,
                PowerUsageTrend = (byte)ProcessPowerUsage.Low,
            },
            UpdateTime = 53
        };
        netObject.Processes.Add(processItem);

        var buffer = netObject.SerializeByNative(32);
        var desObject = buffer.DeserializeByNative<UpdateActiveProcessList>();

        Assert.Equal(netObject.TotalSize, desObject.TotalSize);
        Assert.NotNull(desObject.Processes);
        Assert.NotNull(desObject.Processes[0].ProcessData);
        // ProcessItemData总共60位，序列化后应该占8个字节
        Assert.Equal(8, desObject.Processes[0].Data?.Length);
        Assert.Equal(processItem.ProcessData.Cpu, desObject.Processes[0].ProcessData?.Cpu);
    }
}