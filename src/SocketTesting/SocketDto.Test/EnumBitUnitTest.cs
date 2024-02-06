using System.ComponentModel;

namespace SocketDto.Test;

public class EnumBitUnitTest
{
    [Fact]
    public void Test_SerializeProcessItemData_Success()
    {
        Assert.Equal(0, (int)(UpdateStatus.Normal));
        Assert.Equal(1, (int)(UpdateStatus.Overtime));
        Assert.Equal(2, (int)(UpdateStatus.OverLimit));
        Assert.Equal(3, (int)(UpdateStatus.Overtime | UpdateStatus.OverLimit));
        Assert.Equal(4, (int)(UpdateStatus.Manual));
        Assert.Equal(5, (int)(UpdateStatus.Overtime | UpdateStatus.Manual));
        Assert.Equal(6, (int)(UpdateStatus.OverLimit | UpdateStatus.Manual));
        Assert.Equal(7, (int)(UpdateStatus.Overtime | UpdateStatus.OverLimit | UpdateStatus.Manual));
    }
}

[Flags]
internal enum UpdateStatus
{
    [Description("正常")] Normal = 0,
    [Description("超时")] Overtime = 1,
    [Description("超限")] OverLimit = 2,
    [Description("手动制动")] Manual = 4
}