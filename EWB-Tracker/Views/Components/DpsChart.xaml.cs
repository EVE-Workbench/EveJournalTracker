using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using EWB_Tracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Services;
using SharedLibrary.Models;

namespace EWB_Tracker.Views.Components;

public partial class DpsChart : UserControl
{
    private DpsChartViewModel? _vm;
    private FileWatcherService? _watcher;

    public DpsChart(DpsChartViewModel vm, FileWatcherService watcher)
    {
        InitializeComponent();
        WireUp(vm, watcher);
    }

    // Parameter‑less constructor used by XAML
    public DpsChart()
    {
        InitializeComponent();

        if (!DesignerProperties.GetIsInDesignMode(this))
            Loaded += OnLoaded;                   // resolve DI after the control is loaded
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        var sp = App.ServiceProvider ?? throw new InvalidOperationException(
            "ServiceProvider was not initialised.");

        var vm      = sp.GetRequiredService<DpsChartViewModel>();
        var watcher = sp.GetRequiredService<FileWatcherService>();

        WireUp(vm, watcher);
    }

    private void WireUp(DpsChartViewModel vm, FileWatcherService watcher)
    {
        _vm      = vm;
        _watcher = watcher;
        DataContext = _vm;

        if (_watcher == null) return;
        
        _watcher.OnNewLogEvent += Watcher_OnNewLogEvent;

        Unloaded += (_, __) =>
        {
            _watcher.OnNewLogEvent -= Watcher_OnNewLogEvent;
            _vm?.Dispose();
        };
    }

    private void Watcher_OnNewLogEvent(object? sender, LogEvent e)
        => _vm?.ProcessLog(e);
}