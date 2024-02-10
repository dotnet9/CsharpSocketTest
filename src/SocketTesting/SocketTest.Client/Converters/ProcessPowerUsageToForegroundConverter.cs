using Avalonia.Data.Converters;
using Avalonia.Media;
using SocketDto.Enums;
using System;
using System.Globalization;

namespace SocketTest.Client.Converters;

public class ProcessPowerUsageToForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return Brushes.Green;

        var powerUsageType =
            (PowerUsage)Enum.Parse(typeof(PowerUsage), value.ToString()!);
        return powerUsageType switch
        {
            PowerUsage.VeryLow or PowerUsage.Low => Brushes.LightGreen,
            PowerUsage.Moderate => Brushes.Green,
            PowerUsage.High => Brushes.DarkOrange,
            _ => Brushes.Red
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}