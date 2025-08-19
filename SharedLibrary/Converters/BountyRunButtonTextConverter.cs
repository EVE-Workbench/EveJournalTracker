using System.Globalization;
using System.Windows.Data;

namespace SharedLibrary.Converters;

public class BountyRunButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "Stop Bounty Run" : "Start Bounty Run";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}