using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace UI.avalonia.Converters;

public class CharacterIdToImageSourceConverter : IValueConverter
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int characterId && characterId > 0)
        {
            var url = $"https://images.evetech.net/characters/{characterId}/portrait?size=128";

            // Start loading the image asynchronously
            var task = Task.Run(async () =>
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        using var memoryStream = new MemoryStream(bytes);
                        return new Bitmap(memoryStream);
                    }
                }
                catch
                {
                    // Ignore errors
                }
                return null;
            });

            // Return the task - Avalonia can handle Task<Bitmap>
            return task;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
