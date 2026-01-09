using System;
using System.Globalization;
using System.Windows.Data;

namespace EWB_Tracker.Converters;

public class BountyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int bounty)
        {
            return $"{bounty:N0} ISK";
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
