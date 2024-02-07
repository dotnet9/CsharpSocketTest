using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace SocketTest.Client.Converters;

public class UsageToBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || !short.TryParse(value.ToString(), out var bValue))
        {
            return Brushes.Green;
        }

        var dValue = bValue * 1.0 / 10;
        return dValue switch
        {
            < 5 => Brushes.LightGreen,
            < 10 => Brushes.Green,
            < 20 => Brushes.DarkOrange,
            _ => Brushes.Red
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}