using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SharedLibrary.Enums;

namespace UI.avalonia.Converters;

public class LogEventTypeToBrushConverter : IValueConverter
{
    private static readonly IBrush Jump = new SolidColorBrush(Color.Parse("#3B82F6"));
    private static readonly IBrush Bounty = new SolidColorBrush(Color.Parse("#22A559"));
    private static readonly IBrush Combat = new SolidColorBrush(Color.Parse("#D9822B"));
    private static readonly IBrush Other = new SolidColorBrush(Color.Parse("#6B7280"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            LogEventType.Jump => Jump,
            LogEventType.Bounty => Bounty,
            LogEventType.Combat => Combat,
            _ => Other,
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
