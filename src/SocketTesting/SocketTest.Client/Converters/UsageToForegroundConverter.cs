﻿using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SocketTest.Client.Converters;

public class UsageToForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || !short.TryParse(value.ToString(), out var bValue)) return Brushes.Green;

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