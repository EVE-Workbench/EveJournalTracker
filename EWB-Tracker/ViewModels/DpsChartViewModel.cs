using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Defaults;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SharedLibrary.Models;

namespace EWB_Tracker.ViewModels;

public sealed class DpsChartViewModel : INotifyPropertyChanged, IDisposable
{
    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    #endregion

    private readonly TimeSpan _window = TimeSpan.FromSeconds(30);
    private readonly Dispatcher _ui = Application.Current.Dispatcher;
    private readonly CultureInfo _ci = CultureInfo.InvariantCulture;

    private readonly ObservableCollection<ObservablePoint> _dpsIn = new();
    private readonly ObservableCollection<ObservablePoint> _dpsOut = new();

    public ISeries[] Series { get; }
    private readonly DateTimeAxis _customAxis;
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; }

    public object Sync { get; } = new object();

    public bool IsReading { get; set; } = true;
    
    public SolidColorPaint LegendTextPaint { get; set; } =
        new SolidColorPaint
        {
            Color = new SKColor(230, 230, 230),
        };

    public DpsChartViewModel()
    {
        Series =
        [
            new LineSeries<ObservablePoint>
            {
                Values = _dpsIn,
                Name = "DPS In",
                GeometrySize = 0,
                Stroke = new SolidColorPaint { Color = SKColors.OrangeRed, StrokeThickness = 2 }
            },
            new LineSeries<ObservablePoint>
            {
                Values = _dpsOut,
                Name = "DPS Out",
                GeometrySize = 0,
                Stroke = new SolidColorPaint { Color = SKColors.DodgerBlue, StrokeThickness = 2 }
            }
        ];

        _customAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
        {
            CustomSeparators = GetSeparators(),
            AnimationsSpeed = TimeSpan.FromMilliseconds(0),
            SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(100))
        };
        XAxes = [_customAxis];

        YAxes =
        [
            new Axis
            {
                Labeler = v => v.ToString("0"),
                MinLimit = 0
            }
        ];

        _ = ReadData();
    }

    private async Task ReadData()
    {
        while (IsReading)
        {
            await Task.Delay(100);

            lock (Sync)
            {
                var now = DateTime.Now;

                Trim(_dpsIn, now);
                Trim(_dpsOut, now);

                var axis = XAxes[0];
                axis.MinLimit = now.Subtract(_window).Ticks;
                axis.MaxLimit = now.Ticks;

                // update the Seperators position every update 
                _customAxis.CustomSeparators = GetSeparators();
            }
        }
    }

    public void ProcessLog(LogEvent log)
    {
        if (log.Value is null) return;
        if (!double.TryParse(log.Value, NumberStyles.Float, _ci, out double val)) return;

        // transform the log time to the current time zone (logs are UTC+0)
        var now = TimeZoneInfo.ConvertTimeFromUtc(log.Timestamp, TimeZoneInfo.Local);

        // If the entries are older than the time window, ignore them.
        if (now < DateTime.Now.Subtract(_window)) return;

        _ui.InvokeAsync(() =>
        {
            ObservableCollection<ObservablePoint>? target = log.DamageType switch
            {
                "DpsIn" => _dpsIn,
                "DpsOut" => _dpsOut,
                _ => null
            };
            if (target is null) return;

            target.Add(new ObservablePoint(now.Ticks, val));

            Trim(_dpsIn, now);
            Trim(_dpsOut, now);

            // move X‑axis window
            var axis = (Axis)XAxes[0];
            axis.MinLimit = now.Subtract(_window).Ticks;
            axis.MaxLimit = now.Ticks;
        });
    }

    private void Trim(ObservableCollection<ObservablePoint> col, DateTime now)
    {
        while (col.Count > 0 && col[0].X != null && (now - new DateTime((long)col[0].X)) > _window + TimeSpan.FromSeconds(5))
            col.RemoveAt(0);
    }

    private static string Formatter(DateTime date)
    {
        var secsAgo = (DateTime.Now - date).TotalSeconds;

        return secsAgo < 1
            ? "now"
            : $"{secsAgo:N0}s";
    }

    private static double[] GetSeparators()
    {
        var now = DateTime.Now;

        return
        [
            now.AddSeconds(-25).Ticks,
            now.AddSeconds(-20).Ticks,
            now.AddSeconds(-15).Ticks,
            now.AddSeconds(-10).Ticks,
            now.AddSeconds(-5).Ticks,
            now.Ticks
        ];
    }

    public void Dispose()
    {
    }
}