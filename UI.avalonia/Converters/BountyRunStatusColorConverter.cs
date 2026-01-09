using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace UI.avalonia.Converters;

public class BountyRunStatusColorConverter : IValueConverter
{
    public static readonly BountyRunStatusColorConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCompleted)
        {
            if (isCompleted)
            {
                // Green for completed
                return Color.FromRgb(40, 167, 69);
            }
            else
            {
                // Orange for active
                return Color.FromRgb(255, 193, 7);
            }
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
