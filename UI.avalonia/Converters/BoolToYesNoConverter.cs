using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UI.avalonia.Converters;

public class BoolToYesNoConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Yes" : "No";
        }
        return "No";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
