using System;
using System.Globalization;
using System.Windows.Data;

namespace EWB_Tracker.Converters;

public class BountyRunStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCompleted)
        {
            return isCompleted ? "COMPLETED" : "ACTIVE";
        }
        return "UNKNOWN";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
