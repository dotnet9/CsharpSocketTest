namespace SocketDto.Test;

public class ResponseProcessListUnitTest
{
    [Fact]
    public void Test_SerializeProcessItemData_Success()
    {
        var data = new ProcessItemData
        {
            CPU = 112,
            Memory = 325,
            Disk = 23,
            Network = 593,
            GPU = 253,
            GPUEngine = (byte)GpuEngine.None,
            PowerUsage = (byte)ProcessPowerUsage.Low,
            PowerUsageTrend = (byte)ProcessPowerUsage.Low,
            Type = (byte)ProcessType.Application,
            Status = (byte)ProcessStatus.Running
        };

        var buffer = data.FieldObjectBuffer();
        var desData = buffer.ToFieldObject<ProcessItemData>();

        // ProcessItemData�ܹ�60λ�����л���Ӧ��ռ8���ֽ�
        Assert.Equal(8, buffer.Length);
        Assert.Equal(data.CPU, desData.CPU);
        Assert.Equal(data.Status, desData.Status);
    }

    [Fact]
    public void Test_SerializeResponseProcessList_Success()
    {
        var netObject = new ResponseProcessList()
        {
            TaskId = 3,
            TotalSize = 200,
            PageSize = 3,
            PageCount = 67,
            PageIndex = 1,
            Processes = new List<ProcessItem>()
        };
        var processItem = new ProcessItem
        {
            PID = 1,
            Name = "Dotnet������",
            Publisher = "ɳĮ��ͷ����",
            CommandLine = "dotnet Dotnetools.com",
            ProcessData = new ProcessItemData()
            {
                CPU = 112,
                Memory = 325,
                Disk = 23,
                Network = 593,
                GPU = 253,
                GPUEngine = (byte)GpuEngine.None,
                PowerUsage = (byte)ProcessPowerUsage.Low,
                PowerUsageTrend = (byte)ProcessPowerUsage.Low,
                Type = (byte)ProcessType.Application,
                Status = (byte)ProcessStatus.Running
            },
            LastUpdateTime = 23,
            UpdateTime = 53
        };
        netObject.Processes.Add(processItem);

        var buffer = netObject.Serialize(32);
        var desObject = buffer.Deserialize<ResponseProcessList>();

        Assert.Equal(netObject.TotalSize, desObject.TotalSize);
        Assert.NotNull(desObject.Processes);
        Assert.NotNull(desObject.Processes[0].ProcessData);
        // ProcessItemData�ܹ�60λ�����л���Ӧ��ռ8���ֽ�
        Assert.Equal(8, desObject.Processes[0].Data?.Length);
        Assert.Equal(processItem.ProcessData.CPU, desObject.Processes[0].ProcessData?.CPU);
        Assert.Equal(processItem.LastUpdateTime, desObject.Processes[0].LastUpdateTime);
    }
}