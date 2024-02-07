using Avalonia.Data.Converters;
using Avalonia.Media;
using SocketDto;
using System;
using System.Globalization;
using SocketTest.Common;

namespace SocketTest.Client.Converters;

public class ProcessPowerUsageToFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return Brushes.Green;
        }

        var powerUsageType =
            (ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), value.ToString()!);
        return powerUsageType.Description();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}