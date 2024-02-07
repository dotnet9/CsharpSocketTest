namespace SocketDto.Test;

public class UpdateRealtimeProcessListUnitTest
{
    [Fact]
    public void Test_SerializeActiveProcessItemData_Success()
    {
        var data = new RealtimeProcessItemData
        {
            Cpu = 689,
            Memory = 489,
            Disk = 256,
            Network = 782,
        };

        var buffer = data.FieldObjectBuffer();
        var desData = buffer.ToFieldObject<RealtimeProcessItemData>();

        // ActiveProcessItemData总共57位，序列化后应该占8个字节
        Assert.Equal(8, buffer.Length);
        Assert.Equal(data.Cpu, desData.Cpu);
    }


    [Fact]
    public void Test_SerializeUpdateActiveProcessList_Success()
    {
        var netObject = new UpdateRealtimeProcessList
        {
            TotalSize = 200,
            PageSize = 3,
            PageCount = 67,
            PageIndex = 1,
            Processes = Enumerable.Range(0, 4).Select(index => new RealtimeProcessItem
            {
                ProcessData = new RealtimeProcessItemData
                {
                    Cpu = 689,
                    Memory = 489,
                    Disk = 256,
                    Network = 782
                }
            }).ToList()
        };

        var buffer = netObject.SerializeByNative(32);
        var desObject = buffer.DeserializeByNative<UpdateRealtimeProcessList>();

        Assert.Equal(netObject.TotalSize, desObject.TotalSize);
        Assert.NotNull(desObject.Processes);
        Assert.NotNull(desObject.Processes[0].ProcessData);
        // ProcessItemData总共60位，序列化后应该占8个字节
        Assert.Equal(8, desObject.Processes[0].Data?.Length);
        Assert.Equal(netObject.Processes[0].ProcessData!.Cpu, desObject.Processes[0].ProcessData?.Cpu);
    }
}