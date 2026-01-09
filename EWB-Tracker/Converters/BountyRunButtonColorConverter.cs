using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EWB_Tracker.Converters;

public class BountyRunButtonColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            return new SolidColorBrush(Color.FromRgb(220, 53, 69));
        }
        else
        {
            return new SolidColorBrush(Color.FromRgb(0, 120, 160));
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
