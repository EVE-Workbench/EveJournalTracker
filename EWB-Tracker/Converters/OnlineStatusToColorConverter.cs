using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EWB_Tracker.Converters;

public class OnlineStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isOnline)
        {
            return isOnline
                ? new SolidColorBrush(Color.FromRgb(0, 255, 0))
                : new SolidColorBrush(Color.FromRgb(128, 128, 128));
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
