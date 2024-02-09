namespace SocketDto.Test;

public class UpdateGeneralProcessListUnitTest
{
    [Fact]
    public void Test_SerializeProcessItemData_Success()
    {
        Assert.Equal(0, (int)ProcessAlarmStatus.Normal);
        Assert.Equal(1, (int)ProcessAlarmStatus.Overtime);
        Assert.Equal(2, (int)ProcessAlarmStatus.OverLimit);
        Assert.Equal(3, (int)(ProcessAlarmStatus.Overtime | ProcessAlarmStatus.OverLimit));
        Assert.Equal(4, (int)ProcessAlarmStatus.UserChanged);
        Assert.Equal(5, (int)(ProcessAlarmStatus.Overtime | ProcessAlarmStatus.UserChanged));
        Assert.Equal(6, (int)(ProcessAlarmStatus.OverLimit | ProcessAlarmStatus.UserChanged));
        Assert.Equal(7,
            (int)(ProcessAlarmStatus.Overtime | ProcessAlarmStatus.OverLimit | ProcessAlarmStatus.UserChanged));
    }
}