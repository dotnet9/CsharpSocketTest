using SocketDto.Enums;

namespace SocketDto.Test;

public class UpdateGeneralProcessListUnitTest
{
    [Fact]
    public void Test_SerializeProcessItemData_Success()
    {
        Assert.Equal(0, (int)AlarmStatus.Normal);
        Assert.Equal(1, (int)AlarmStatus.Overtime);
        Assert.Equal(2, (int)AlarmStatus.OverLimit);
        Assert.Equal(3, (int)(AlarmStatus.Overtime | AlarmStatus.OverLimit));
        Assert.Equal(4, (int)AlarmStatus.UserChanged);
        Assert.Equal(5, (int)(AlarmStatus.Overtime | AlarmStatus.UserChanged));
        Assert.Equal(6, (int)(AlarmStatus.OverLimit | AlarmStatus.UserChanged));
        Assert.Equal(7,
            (int)(AlarmStatus.Overtime | AlarmStatus.OverLimit | AlarmStatus.UserChanged));
        Assert.Equal(7,
            (int)(AlarmStatus.Overtime | AlarmStatus.Overtime | AlarmStatus.Overtime | AlarmStatus.OverLimit |
                  AlarmStatus.Overtime | AlarmStatus.UserChanged));
        const AlarmStatus status = AlarmStatus.Overtime | AlarmStatus.OverLimit | AlarmStatus.UserChanged;
        Assert.Equal((int)AlarmStatus.Overtime, (int)(AlarmStatus.Overtime & status));
        Assert.Equal((int)AlarmStatus.OverLimit, (int)(AlarmStatus.OverLimit & status));
        Assert.Equal((int)AlarmStatus.UserChanged, (int)(AlarmStatus.UserChanged & status));
    }
}