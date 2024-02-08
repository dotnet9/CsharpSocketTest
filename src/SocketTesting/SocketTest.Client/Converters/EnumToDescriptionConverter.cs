using Avalonia.Data.Converters;
using SocketTest.Common;
using System;
using System.Globalization;

namespace SocketTest.Client.Converters;

public class EnumToDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue) return value;

        return enumValue.Description();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}