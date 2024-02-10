using SocketDto.Enums;

namespace SocketDto.Test;

public class ResponseProcessListUnitTest
{
    [Fact]
    public void Test_SerializeResponseProcessList_Success()
    {
        var netObject = new ResponseProcessList
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
            Pid = 1,
            Name = "Dotnet工具箱",
            Type = (byte)ProcessType.Application,
            ProcessStatus = (byte)ProcessStatus.Running,
            Publisher = "沙漠尽头的狼",
            CommandLine = "dotnet Dotnetools.com",
            Cpu = 112,
            Memory = 325,
            Disk = 23,
            Network = 593,
            Gpu = 253,
            GpuEngine = (byte)GpuEngine.None,
            PowerUsage = (byte)PowerUsage.Low,
            PowerUsageTrend = (byte)PowerUsage.Low,
            LastUpdateTime = 23,
            UpdateTime = 53
        };
        netObject.Processes.Add(processItem);

        var buffer = netObject.Serialize(32);
        var desObject = buffer.Deserialize<ResponseProcessList>();

        Assert.Equal(netObject.TotalSize, desObject.TotalSize);
        Assert.NotNull(desObject.Processes);
        Assert.Equal(processItem.Cpu, desObject.Processes[0].Cpu);
        Assert.Equal(processItem.LastUpdateTime, desObject.Processes[0].LastUpdateTime);
    }


    [Fact]
    public void Test_FormatDouble_Success()
    {
        const double data = 3.2359;
        const string format1 = "00000.00";
        const string format2 = "00.000";
        const string expectedData1 = "00003.24";
        const string expectedData2 = "03.236";

        var formatData1 = data.ToString(format1);
        var formatData2 = data.ToString(format2);

        Assert.Equal(expectedData1, formatData1);
        Assert.Equal(expectedData2, formatData2);
    }
}