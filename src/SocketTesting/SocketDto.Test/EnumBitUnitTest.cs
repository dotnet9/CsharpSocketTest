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
    [Description("����")] Normal = 0,
    [Description("��ʱ")] Overtime = 1,
    [Description("����")] OverLimit = 2,
    [Description("�ֶ��ƶ�")] Manual = 4
}