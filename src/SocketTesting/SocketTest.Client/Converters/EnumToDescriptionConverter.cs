using Avalonia.Data.Converters;
using CodeWF.Tools.Extensions;
using System;
using System.Globalization;

namespace SocketTest.Client.Converters;

public class EnumToDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue) return value;

        return enumValue.GetDescription();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}