using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using SharedLibrary.Enums;
using SharedLibrary.Models;

namespace UI.avalonia.Converters;

public class LogEventSummaryConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LogEvent e)
            return string.Empty;

        return e.Type switch
        {
            LogEventType.Jump => e.Value ?? string.Empty,
            LogEventType.Bounty => $"{e.BountyValue ?? 0:N0} ISK",
            LogEventType.Combat => FormatCombat(e),
            _ => StripPrefix(e.Raw ?? e.Value ?? string.Empty),
        };
    }

    private static string FormatCombat(LogEvent e)
    {
        if (string.IsNullOrWhiteSpace(e.DamageType))
            return StripPrefix(e.Raw ?? string.Empty);

        var parts = new[] { e.DamageType, e.Value, e.DamageQuality }
            .Where(part => !string.IsNullOrWhiteSpace(part));

        return string.Join("  ", parts);
    }

    // Drop the leading "[ timestamp ] " so the message column isn't redundant with the time column.
    private static string StripPrefix(string raw)
    {
        var end = raw.IndexOf("] ", StringComparison.Ordinal);
        return end >= 0 ? raw[(end + 2)..] : raw;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
