using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SocketClient.Converters
{
    public class UsageToBackgroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || !double.TryParse(value.ToString(), out var dValue))
            {
                return Brushes.Green;
            }

            if (dValue < 5)
            {
                return Brushes.LightGreen;
            }

            if (dValue < 10)
            {
                return Brushes.Green;
            }

            if (dValue < 20)
            {
                return Brushes.DarkOrange;
            }

            return Brushes.Red;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}