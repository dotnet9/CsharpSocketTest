using SocketDto.Enums;
using SocketTest.Common;

namespace SocketDto.Test;

public class EnumDescriptionUnitTest
{
    [Fact]
    public void Test_GetEnumDescription_Success()
    {
        var usage = PowerUsage.High;
        var alarmStatus = AlarmStatus.OverLimit | AlarmStatus.UserChanged;

        var usageDescription = usage.Description();
        var alarmStatusDescription = alarmStatus.Description();

        Assert.Equal("高", usageDescription);
        Assert.Equal("超限,切换用户", alarmStatusDescription);

        alarmStatus = AlarmStatus.Normal;
        alarmStatusDescription = alarmStatus.Description();
        Assert.Equal("正常", alarmStatusDescription);
    }
}