using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SharedLibrary.Converters;

public class BountyRunStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            // Green for completed
            return new SolidColorBrush(Color.FromRgb(40, 167, 69));
        }
        else
        {
            // Orange for active
            return new SolidColorBrush(Color.FromRgb(255, 193, 7));
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}