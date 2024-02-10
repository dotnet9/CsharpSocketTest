using Avalonia.Data.Converters;
using Avalonia.Media;
using SocketDto.Enums;
using System;
using System.Globalization;

namespace SocketTest.Client.Converters;

public class AlarmStatusToForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AlarmStatus status) return Brushes.Red;
        return status == AlarmStatus.Normal ? Brushes.Green : Brushes.Red;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}