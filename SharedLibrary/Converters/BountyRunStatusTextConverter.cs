using System.Globalization;
using System.Windows.Data;

namespace SharedLibrary.Converters;

public class BountyRunStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "COMPLETED" : "ACTIVE";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}