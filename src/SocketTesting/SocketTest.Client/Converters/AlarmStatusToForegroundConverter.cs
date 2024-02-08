using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SocketDto;

namespace SocketTest.Client.Converters;

public class AlarmStatusToForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ProcessAlarmStatus status) return Brushes.Red;
        return status == ProcessAlarmStatus.Normal ? Brushes.Green : Brushes.Red;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}