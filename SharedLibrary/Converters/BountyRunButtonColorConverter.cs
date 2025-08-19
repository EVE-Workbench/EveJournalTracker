using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SharedLibrary.Converters;

public class BountyRunButtonColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            // Red color for stop button
            return new SolidColorBrush(Color.FromRgb(220, 53, 69));
        }
        else
        {
            // Use default CTA color for start button
            return new SolidColorBrush(Color.FromRgb(0, 120, 160));
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}