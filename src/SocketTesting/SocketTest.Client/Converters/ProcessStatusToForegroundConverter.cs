using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SocketDto;

namespace SocketTest.Client.Converters;

public class ProcessStatusToForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ProcessStatus status)
        {
            return status switch
            {
                < ProcessStatus.Running => Brushes.CadetBlue,
                > ProcessStatus.Running => Brushes.Green,
                _ => Brushes.Red
            };
        }

        return Brushes.CadetBlue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}