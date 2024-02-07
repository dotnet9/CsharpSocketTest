namespace SocketDto.Test;

public class UpdateGeneralProcessListUnitTest
{
    [Fact]
    public void Test_SerializeProcessItemData_Success()
    {
        Assert.Equal(0, (int)ProcessOtherStatus.Normal);
        Assert.Equal(1, (int)ProcessOtherStatus.Overtime);
        Assert.Equal(2, (int)ProcessOtherStatus.OverLimit);
        Assert.Equal(3, (int)(ProcessOtherStatus.Overtime | ProcessOtherStatus.OverLimit));
        Assert.Equal(4, (int)ProcessOtherStatus.UserChanged);
        Assert.Equal(5, (int)(ProcessOtherStatus.Overtime | ProcessOtherStatus.UserChanged));
        Assert.Equal(6, (int)(ProcessOtherStatus.OverLimit | ProcessOtherStatus.UserChanged));
        Assert.Equal(7,
            (int)(ProcessOtherStatus.Overtime | ProcessOtherStatus.OverLimit | ProcessOtherStatus.UserChanged));
    }
}