﻿using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SocketClient.Converters
{
    public class ProcessPowerUsageToBackgroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Brushes.Green;
            }

            var powerUsageType =
                (ProcessPowerUsage)Enum.Parse(typeof(ProcessPowerUsage), value.ToString()!);
            return powerUsageType switch
            {
                ProcessPowerUsage.VeryLow or ProcessPowerUsage.Low => Brushes.LightGreen,
                ProcessPowerUsage.Moderate => Brushes.Green,
                ProcessPowerUsage.High => Brushes.DarkOrange,
                _ => Brushes.Red,
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}