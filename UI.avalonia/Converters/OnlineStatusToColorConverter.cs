using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace UI.avalonia.Converters
{
    public class OnlineStatusToColorConverter : IValueConverter
    {
        public static readonly OnlineStatusToColorConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOnline)
            {
                return isOnline ? Colors.Green : Colors.Red;
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
