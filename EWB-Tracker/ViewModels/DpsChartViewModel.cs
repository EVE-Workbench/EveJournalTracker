using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Defaults;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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

    private readonly TimeSpan _calculationWindow = TimeSpan.FromSeconds(2); // Time window over which DPS is calculated
    private readonly TimeSpan _decayRate = TimeSpan.FromMilliseconds(500); // Rate at which the DPS value decreases when no new data arrives
    private readonly TimeSpan _displayWindow = TimeSpan.FromSeconds(30); // Time window shown on the chart
    private readonly Dispatcher _ui = Application.Current.Dispatcher; // Dispatcher for UI thread operations
    private readonly CultureInfo _ci = CultureInfo.InvariantCulture; // Culture info for parsing numbers

    private readonly ConcurrentQueue<LogEvent> _dpsInQueue = new(); // queue for incoming damage logs
    private readonly ConcurrentQueue<LogEvent> _dpsOutQueue = new(); // queue for outgoing damage logs

    private readonly ObservableCollection<ObservablePoint> _dpsInValues = new(); // Collection to store DPS In values for the chart
    private readonly ObservableCollection<ObservablePoint> _dpsOutValues = new(); // Collection to store DPS Out values for the chart

    public ISeries[] Series { get; } // Array of series to display on the chart
    private readonly DateTimeAxis _customAxis; // Custom X-axis for displaying time
    public Axis[] XAxes { get; set; } // Array of X-axes for the chart
    public Axis[] YAxes { get; } // Array of Y-axes for the chart

    public object Sync { get; } = new object(); // Object for thread synchronization

    public bool IsReading { get; set; } = true; // Flag to control the data processing loops

    public SolidColorPaint LegendTextPaint { get; set; } =
        new SolidColorPaint
        {
            Color = new SKColor(230, 230, 230),
        };

    public DpsChartViewModel()
    {
        // Initialize the series for DPS In and DPS Out
        Series =
        [
            new LineSeries<ObservablePoint>
            {
                Values = _dpsInValues,
                Name = "DPS In",
                GeometrySize = 0,
                Stroke = new SolidColorPaint { Color = SKColors.OrangeRed, StrokeThickness = 2 }
            },
            new LineSeries<ObservablePoint>
            {
                Values = _dpsOutValues,
                Name = "DPS Out",
                GeometrySize = 0,
                Stroke = new SolidColorPaint { Color = SKColors.DodgerBlue, StrokeThickness = 2 }
            }
        ];

        // Initialize the custom X-axis to display time
        _customAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
        {
            CustomSeparators = GetSeparators(),
            AnimationsSpeed = TimeSpan.FromMilliseconds(0),
            SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(100))
        };
        XAxes = [_customAxis];

        // Initialize the Y-axis for DPS values
        YAxes =
        [
            new Axis
            {
                Labeler = v => v.ToString("0"),
                MinLimit = 0
            }
        ];

        // Start the background tasks for processing DPS and updating the chart
        _ = ProcessDps();
        _ = UpdateChart();
    }

    private async Task ProcessDps()
    {
        double lastDpsIn = 0;
        DateTime lastDpsInTime = DateTime.MinValue;
        double lastDpsOut = 0;
        DateTime lastDpsOutTime = DateTime.MinValue;

        while (IsReading)
        {
            await Task.Delay(50);

            var now = DateTime.Now;
            double currentDpsIn = 0;
            double currentDpsOut = 0;

            // Bereken DPS In
            var inEvents = _dpsInQueue.Where(log =>
                now - TimeZoneInfo.ConvertTimeFromUtc(log.Timestamp, TimeZoneInfo.Local) < _calculationWindow).ToList();
            double totalDamageIn = 0;
            foreach (var log in inEvents)
            {
                if (double.TryParse(log.Value, NumberStyles.Float, _ci, out double val))
                {
                    totalDamageIn += val;
                }
            }
            currentDpsIn = inEvents.Any() ? totalDamageIn / _calculationWindow.TotalSeconds : 0;
            while (_dpsInQueue.TryPeek(out var peeked) &&
                   now - TimeZoneInfo.ConvertTimeFromUtc(peeked.Timestamp, TimeZoneInfo.Local) >= _calculationWindow)
            {
                _dpsInQueue.TryDequeue(out _);
            }

            // Bereken DPS Out
            var outEvents = _dpsOutQueue.Where(log =>
                now - TimeZoneInfo.ConvertTimeFromUtc(log.Timestamp, TimeZoneInfo.Local) < _calculationWindow).ToList();
            double totalDamageOut = 0;
            foreach (var log in outEvents)
            {
                if (double.TryParse(log.Value, NumberStyles.Float, _ci, out double val))
                {
                    totalDamageOut += val;
                }
            }
            currentDpsOut = outEvents.Any() ? totalDamageOut / _calculationWindow.TotalSeconds : 0;
            while (_dpsOutQueue.TryPeek(out var peeked) &&
                   now - TimeZoneInfo.ConvertTimeFromUtc(peeked.Timestamp, TimeZoneInfo.Local) >= _calculationWindow)
            {
                _dpsOutQueue.TryDequeue(out _);
            }

            _ui.InvokeAsync(() =>
            {
                lock (Sync)
                {
                    // Afhandeling van DPS In
                    if (currentDpsIn == 0 && lastDpsIn > 0 && (now - lastDpsInTime) > _decayRate)
                    {
                        lastDpsIn -= lastDpsIn / (_decayRate.TotalMilliseconds / 50);
                        if (lastDpsIn < 2) lastDpsIn = 0; // Direct naar 0 als onder de 1
                    }
                    else if (currentDpsIn > 0)
                    {
                        lastDpsIn = currentDpsIn;
                        lastDpsInTime = now;
                    }

                    if (lastDpsIn < 0) lastDpsIn = 0; // Voorkom negatieve waarden
                    _dpsInValues.Add(new ObservablePoint(now.Ticks, lastDpsIn));

                    // Afhandeling van DPS Out
                    if (currentDpsOut == 0 && lastDpsOut > 0 && (now - lastDpsOutTime) > _decayRate)
                    {
                        lastDpsOut -= lastDpsOut / (_decayRate.TotalMilliseconds / 50);
                        if (lastDpsOut < 2) lastDpsOut = 0; // Direct naar 0 als onder de 1
                    }
                    else if (currentDpsOut > 0)
                    {
                        lastDpsOut = currentDpsOut;
                        lastDpsOutTime = now;
                    }

                    if (lastDpsOut < 0) lastDpsOut = 0; // Voorkom negatieve waarden
                    _dpsOutValues.Add(new ObservablePoint(now.Ticks, lastDpsOut));

                    Trim(_dpsInValues, now, _displayWindow);
                    Trim(_dpsOutValues, now, _displayWindow);
                }
            });
        }
    }

    private async Task UpdateChart()
    {
        while (IsReading)
        {
            await Task.Delay(200); // Periodically update the chart

            var now = DateTime.Now;

            _ui.InvokeAsync(() =>
            {
                lock (Sync)
                {
                    // Update the X-axis limits to show the data within the display window
                    var axis = XAxes[0];
                    axis.MinLimit = now.Subtract(_displayWindow).Ticks;
                    axis.MaxLimit = now.Ticks;

                    // Update the separators on the X-axis
                    _customAxis.CustomSeparators = GetSeparators();
                }
            });
        }
    }

    public void ProcessLog(LogEvent log)
    {
        if (log.Value is null) return;

        // Transform the log time to the current time zone (logs are UTC+0)
        var now = TimeZoneInfo.ConvertTimeFromUtc(log.Timestamp, TimeZoneInfo.Local);

        // If the entry is within the display window, add it to the appropriate queue for DPS calculation
        if (now >= DateTime.Now.Subtract(_displayWindow))
        {
            switch (log.DamageType)
            {
                case "DpsIn":
                    _dpsInQueue.Enqueue(log);
                    break;
                case "DpsOut":
                    _dpsOutQueue.Enqueue(log);
                    break;
            }
        }
    }

    private void Trim(ObservableCollection<ObservablePoint> col, DateTime now, TimeSpan window)
    {
        // Remove data points that are older than the display window plus a small buffer
        while (col.Count > 0 && col[0].X != null &&
               (now - new DateTime((long)col[0].X)) > window + TimeSpan.FromSeconds(1))
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
        IsReading = false; // Stop the background processing loops
    }
}