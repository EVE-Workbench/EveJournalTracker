using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UI.avalonia.Converters;

public class ZeroToCollapsedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count && count == 0)
            return false;

        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
