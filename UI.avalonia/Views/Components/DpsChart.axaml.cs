using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using UI.avalonia.ViewModels;
using UI.avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Services;
using SharedLibrary.Models;

namespace UI.avalonia.Views.Components;

public partial class DpsChart : UserControl
{
    private DpsChartViewModel? _vm;
    private FileWatcherService? _watcher;
    private DpsOverlayWindow? _overlay;

    public DpsChart(DpsChartViewModel vm, FileWatcherService watcher)
    {
        InitializeComponent();
        WireUp(vm, watcher);
    }

    // Parameter-less constructor used by XAML
    public DpsChart()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            Loaded += OnLoaded;
        }
    }

    private void OnLoaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        var sp = App.ServiceProvider ?? throw new InvalidOperationException(
            "ServiceProvider was not initialised.");

        var vm = sp.GetRequiredService<DpsChartViewModel>();
        var watcher = sp.GetRequiredService<FileWatcherService>();

        WireUp(vm, watcher);
    }

    private void WireUp(DpsChartViewModel vm, FileWatcherService watcher)
    {
        _vm = vm;
        _watcher = watcher;
        DataContext = _vm;

        if (_watcher == null) return;

        _watcher.OnNewLogEvent += Watcher_OnNewLogEvent;

        Unloaded += (_, __) =>
        {
            if (_watcher != null)
                _watcher.OnNewLogEvent -= Watcher_OnNewLogEvent;
            _vm?.Dispose();
        };
    }

    private void Watcher_OnNewLogEvent(object? sender, LogEvent e)
        => _vm?.ProcessLog(e);

    private void OnPopOut(object? sender, RoutedEventArgs e)
    {
        if (_vm is null)
            return;

        if (_overlay is not null)
        {
            _overlay.Activate();
            return;
        }

        _overlay = new DpsOverlayWindow { DataContext = _vm };
        _overlay.Closed += (_, __) => _overlay = null;
        _overlay.Show();
    }
}
