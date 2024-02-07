using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SocketTest.Client.Converters;

public class UsageToFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || !short.TryParse(value.ToString(), out var bValue)) return Brushes.Green;

        var dValue = bValue * 1.0 / 1000;
        return dValue.ToString("P1");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}