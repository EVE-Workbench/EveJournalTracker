using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace UI.avalonia.Controls;

public sealed class DpsSeries(IBrush stroke)
{
    public IBrush Stroke { get; } = stroke;
    public List<double> Values { get; } = [];
}

/// <summary>
/// Lightweight real-time line graph (PyEveLiveDPS style). Renders one polyline per
/// series over a scrolling window with an auto-scaling Y axis. The owner mutates each
/// series' <see cref="DpsSeries.Values"/> in place and bumps <see cref="Revision"/> to
/// trigger a redraw.
/// </summary>
public sealed class DpsGraph : Control
{
    private static readonly IBrush GridBrush = new SolidColorBrush(Color.Parse("#14FFFFFF"));
    private static readonly IBrush LabelBrush = new SolidColorBrush(Color.Parse("#FF8A7E6B"));
    private static readonly Typeface LabelTypeface = new("Consolas");

    public static readonly StyledProperty<IReadOnlyList<DpsSeries>?> SeriesProperty =
        AvaloniaProperty.Register<DpsGraph, IReadOnlyList<DpsSeries>?>(nameof(Series));

    public static readonly StyledProperty<int> RevisionProperty =
        AvaloniaProperty.Register<DpsGraph, int>(nameof(Revision));

    public static readonly StyledProperty<int> CapacityProperty =
        AvaloniaProperty.Register<DpsGraph, int>(nameof(Capacity), 150);

    static DpsGraph()
    {
        AffectsRender<DpsGraph>(SeriesProperty, RevisionProperty);
    }

    public IReadOnlyList<DpsSeries>? Series
    {
        get => GetValue(SeriesProperty);
        set => SetValue(SeriesProperty, value);
    }

    public int Revision
    {
        get => GetValue(RevisionProperty);
        set => SetValue(RevisionProperty, value);
    }

    public int Capacity
    {
        get => GetValue(CapacityProperty);
        set => SetValue(CapacityProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        const double padLeft = 40, padTop = 8, padRight = 10, padBottom = 6;
        var plot = new Rect(
            padLeft, padTop,
            Math.Max(0, bounds.Width - padLeft - padRight),
            Math.Max(0, bounds.Height - padTop - padBottom));

        if (plot.Width <= 0 || plot.Height <= 0)
            return;

        var max = NiceCeiling(ObservedMax());
        DrawGrid(context, plot, max);

        var series = Series;
        if (series is null)
            return;

        var capacity = Math.Max(2, Capacity);
        foreach (var s in series)
            DrawSeries(context, plot, s, max, capacity);
    }

    private double ObservedMax()
    {
        var max = 0.0;
        if (Series is { } series)
            foreach (var s in series)
                foreach (var v in s.Values)
                    if (v > max)
                        max = v;
        return max;
    }

    private void DrawGrid(DrawingContext context, Rect plot, double max)
    {
        var pen = new Pen(GridBrush, 1);
        const int lines = 4;
        for (var i = 0; i <= lines; i++)
        {
            var fraction = i / (double)lines;
            var y = plot.Bottom - fraction * plot.Height;
            context.DrawLine(pen, new Point(plot.Left, y), new Point(plot.Right, y));

            var value = max * fraction;
            var text = new FormattedText(
                FormatTick(value), CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                LabelTypeface, 10, LabelBrush);
            context.DrawText(text, new Point(plot.Left - text.Width - 6, y - text.Height / 2));
        }
    }

    private static void DrawSeries(DrawingContext context, Rect plot, DpsSeries s, double max, int capacity)
    {
        var values = s.Values;
        if (values.Count < 2)
            return;

        var stepX = plot.Width / (capacity - 1);
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            for (var i = 0; i < values.Count; i++)
            {
                var x = plot.Right - (values.Count - 1 - i) * stepX;
                var y = plot.Bottom - Math.Clamp(values[i] / max, 0, 1) * plot.Height;
                var point = new Point(x, y);
                if (i == 0)
                    ctx.BeginFigure(point, isFilled: false);
                else
                    ctx.LineTo(point);
            }
            ctx.EndFigure(false);
        }

        context.DrawGeometry(null, new Pen(s.Stroke, 1.6, lineJoin: PenLineJoin.Round), geometry);
    }

    private static double NiceCeiling(double value)
    {
        if (value <= 100)
            return 100;

        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
        var normalized = value / magnitude;
        var nice = normalized switch
        {
            <= 1 => 1,
            <= 2 => 2,
            <= 2.5 => 2.5,
            <= 5 => 5,
            _ => 10
        };
        return nice * magnitude;
    }

    private static string FormatTick(double value) =>
        value >= 1000
            ? (value / 1000).ToString("0.#", CultureInfo.InvariantCulture) + "k"
            : value.ToString("0", CultureInfo.InvariantCulture);
}
