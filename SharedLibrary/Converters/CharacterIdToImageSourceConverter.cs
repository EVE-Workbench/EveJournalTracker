using System.Globalization;
using System.Windows.Data;

namespace SharedLibrary.Converters;

public class CharacterIdToImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int characterId)
        {
            return $"https://images.evetech.net/characters/{characterId}/portrait";
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}