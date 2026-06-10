using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SharedLibrary.Aggregation;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using UI.avalonia.Controls;

namespace UI.avalonia.ViewModels;

public sealed partial class DpsChartViewModel : ViewModelBase, IDisposable
{
    // ~20s of history at the 30fps sample rate; dense points keep scrolling smooth.
    private const int GraphCapacity = 600;

    // Exponential smoothing eases the stair-stepping that bursty log writes cause.
    private const double Smoothing = 0.15;

    private readonly LiveDpsTracker _dps = new();

    private double _emaDealt;
    private double _emaReceived;

    private readonly DpsSeries _dealtSeries = new(new SolidColorBrush(Color.Parse("#FF1E90FF")));
    private readonly DpsSeries _receivedSeries = new(new SolidColorBrush(Color.Parse("#FFFF4500")));

    private readonly DispatcherTimer _refreshTimer;

    [ObservableProperty] private double _dpsDealt;
    [ObservableProperty] private double _dpsReceived;
    [ObservableProperty] private long _totalDealt;
    [ObservableProperty] private long _totalReceived;
    [ObservableProperty] private int _graphRevision;

    public int GraphPointCapacity => GraphCapacity;
    public IReadOnlyList<DpsSeries> Series => [_dealtSeries, _receivedSeries];

    public DpsChartViewModel()
    {
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _refreshTimer.Tick += (_, _) => Refresh(DateTime.UtcNow);
        _refreshTimer.Start();
    }

    public void ProcessLog(LogEvent log)
    {
        // Old log lines read at startup must never be replayed through the live meter.
        if (log.IsHistorical)
            return;

        if (!TryDirection(log.DamageType, out var direction))
            return;

        if (!TryAmount(log.Value, out var amount))
            return;

        // Stamp arrival with wall-clock time so the sliding window decays correctly
        // during live monitoring (EVE log timestamps are UTC, but mixing them with the
        // local clock causes drift). Marshal onto the UI thread so the tracker stays
        // single-threaded against the refresh loop.
        Dispatcher.UIThread.Post(() => _dps.Add(DateTime.UtcNow, direction, amount));
    }

    private void Refresh(DateTime now)
    {
        var sample = _dps.Sample(now);
        _emaDealt += Smoothing * (sample.Dealt - _emaDealt);
        _emaReceived += Smoothing * (sample.Received - _emaReceived);

        DpsDealt = _emaDealt;
        DpsReceived = _emaReceived;
        TotalDealt = _dps.TotalDealt;
        TotalReceived = _dps.TotalReceived;

        Push(_dealtSeries.Values, _emaDealt);
        Push(_receivedSeries.Values, _emaReceived);
        GraphRevision++;
    }

    private static void Push(List<double> values, double value)
    {
        values.Add(value);
        if (values.Count > GraphCapacity)
            values.RemoveAt(0);
    }

    private static bool TryDirection(string? damageType, out DamageDirection direction)
    {
        switch (damageType)
        {
            case "DpsOut":
                direction = DamageDirection.Outgoing;
                return true;
            case "DpsIn":
                direction = DamageDirection.Incoming;
                return true;
            default:
                direction = default;
                return false;
        }
    }

    private static bool TryAmount(string? value, out int amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            return false;

        amount = (int)Math.Round(parsed);
        return amount > 0;
    }

    public void Dispose() => _refreshTimer.Stop();
}
