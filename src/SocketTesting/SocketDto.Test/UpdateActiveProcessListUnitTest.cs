namespace SocketDto.Test;

public class UpdateActiveProcessListUnitTest
{
    [Fact]
    public void Test_SerializeActiveProcessItemData_Success()
    {
        var data = new ActiveProcessItemData
        {
            CPU = 689,
            Memory = 489,
            Disk = 256,
            Network = 782,
            GPU = 493,
            GPUEngine = (byte)GpuEngine.None,
            PowerUsage = (byte)ProcessPowerUsage.Low,
            PowerUsageTrend = (byte)ProcessPowerUsage.Low
        };

        var buffer = data.FieldObjectBuffer();
        var desData = buffer.ToFieldObject<ActiveProcessItemData>();

        // ActiveProcessItemData总共57位，序列化后应该占8个字节
        Assert.Equal(8, buffer.Length);
        Assert.Equal(data.CPU, desData.CPU);
    }


    [Fact]
    public void Test_SerializeUpdateActiveProcessList_Success()
    {
        var netObject = new UpdateActiveProcessList
        {
            TotalSize = 200,
            PageSize = 3,
            PageCount = 67,
            PageIndex = 1,
            Processes = Enumerable.Range(0, 4).Select(index => new ActiveProcessItem
            {
                ProcessData = new ActiveProcessItemData
                {
                    CPU = 689,
                    Memory = 489,
                    Disk = 256,
                    Network = 782,
                    GPU = 493,
                    GPUEngine = (byte)GpuEngine.None,
                    PowerUsage = (byte)ProcessPowerUsage.Low,
                    PowerUsageTrend = (byte)ProcessPowerUsage.Low
                },
                UpdateTime = 53
            }).ToList()
        };

        var buffer = netObject.SerializeByNative(32);
        var desObject = buffer.DeserializeByNative<UpdateActiveProcessList>();

        Assert.Equal(netObject.TotalSize, desObject.TotalSize);
        Assert.NotNull(desObject.Processes);
        Assert.NotNull(desObject.Processes[0].ProcessData);
        // ProcessItemData总共60位，序列化后应该占8个字节
        Assert.Equal(8, desObject.Processes[0].Data?.Length);
        Assert.Equal(netObject.Processes[0].ProcessData!.CPU, desObject.Processes[0].ProcessData?.CPU);
    }
}