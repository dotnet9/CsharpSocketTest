namespace SocketDto.Test;

public class NumberFormatUnitTest
{
    private const int Max = 10000;
    private const double Min = 0.0001;

    [Fact]
    public void Test_NumberToString_Success()
    {
        var strIntMax = NumberToString(20000);
        var strIntCenter = NumberToString(999);
        var strIntZero = NumberToString(0);
        var strDoubleMax = NumberToString(32983.33223);
        var strDoubleCenter = NumberToString(32.358953);
        var strDoubleZero = NumberToString(0.00);
        var strDoubleSmallThan1 = NumberToString(0.330);
        var strDoubleGegative1 = NumberToString(-0.32853);
        var strDoubleGegative2 = NumberToString(-0.0001);
        var strDoubleGegative3 = NumberToString(-0.00001);
        var strDoubleGegative4 = NumberToString(-0.000003235);

        Assert.Equal("0", strIntZero);
    }


    string NumberToString(int number)
    {
        if (Math.Abs(number) > Max)
        {
            return number.ToString("0.00e+00");
        }

        return $"{number}";
    }

    string NumberToString(double number)
    {
        if (number != 0.0 && (Math.Abs(number) > Max || Math.Abs(number) < Min))
        {
            return number.ToString("0.00e+00");
        }

        return $"{number}";
    }
}