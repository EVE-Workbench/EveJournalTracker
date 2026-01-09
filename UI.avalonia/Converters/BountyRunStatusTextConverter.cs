using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UI.avalonia.Converters;

public class BountyRunStatusTextConverter : IValueConverter
{
    public static readonly BountyRunStatusTextConverter Instance = new();

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
