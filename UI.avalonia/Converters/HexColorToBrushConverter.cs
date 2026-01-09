using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace UI.avalonia.Converters;

public class HexColorToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
        {
            try
            {
                return new SolidColorBrush(Color.Parse(hexColor));
            }
            catch
            {
                return new SolidColorBrush(Colors.White);
            }
        }
        return new SolidColorBrush(Colors.White);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
