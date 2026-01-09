using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EWB_Tracker.Converters;

public class BountyRunStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCompleted)
        {
            return isCompleted
                ? new SolidColorBrush(Color.FromRgb(40, 167, 69))   // Green for completed
                : new SolidColorBrush(Color.FromRgb(0, 120, 160));  // Blue for active
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
