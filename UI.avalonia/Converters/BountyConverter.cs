using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UI.avalonia.Converters;

public class BountyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int bounty)
        {
            return $"{bounty:N0} ISK";
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
