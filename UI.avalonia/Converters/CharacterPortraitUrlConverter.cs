using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UI.avalonia.Converters;

/// <summary>
/// Builds the ESI portrait URL for a character id. Keeps the presentation-only URL out of the
/// shared domain model; consumed by AsyncImageLoader's ImageLoader.Source (which takes a URL).
/// </summary>
public sealed class CharacterPortraitUrlConverter : IValueConverter
{
    public static readonly CharacterPortraitUrlConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int characterId && characterId > 0
            ? $"https://images.evetech.net/characters/{characterId}/portrait?size=128"
            : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
