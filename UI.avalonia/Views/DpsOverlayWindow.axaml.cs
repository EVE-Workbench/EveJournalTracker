using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using UI.avalonia.Services;

namespace UI.avalonia.Views;

public partial class DpsOverlayWindow : Window
{
    public static readonly StyledProperty<double> FillOpacityProperty =
        AvaloniaProperty.Register<DpsOverlayWindow, double>(nameof(FillOpacity), 0.9);

    private readonly SolidColorBrush _fillBrush = new(Color.Parse("#1E1E1E"));
    private readonly DispatcherTimer _saveDebounce;
    private OverlayGeometry _geometry = OverlayStore.Load();
    private PixelPoint _lastPosition;
    private bool _ready;

    public DpsOverlayWindow()
    {
        InitializeComponent();

        PanelBorder.Background = _fillBrush;
        FillOpacity = Math.Clamp(_geometry.Opacity, 0, 1);
        _fillBrush.Opacity = FillOpacity;

        if (_geometry.Width >= MinWidth)
            Width = _geometry.Width;
        if (_geometry.Height >= MinHeight)
            Height = _geometry.Height;

        // Coalesce rapid move/resize/opacity changes into a single write shortly after.
        _saveDebounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _saveDebounce.Tick += (_, _) => { _saveDebounce.Stop(); Persist(); };

        // Position is unreliable during teardown, so track the last known good value.
        PositionChanged += (_, e) => { _lastPosition = e.Point; QueueSave(); };
    }

    public double FillOpacity
    {
        get => GetValue(FillOpacityProperty);
        set => SetValue(FillOpacityProperty, value);
    }

    private void QueueSave()
    {
        if (!_ready)
            return;
        _saveDebounce.Stop();
        _saveDebounce.Start();
    }

    private void Persist()
    {
        _geometry.HasPosition = true;
        _geometry.X = _lastPosition.X;
        _geometry.Y = _lastPosition.Y;
        _geometry.Width = Width;
        _geometry.Height = Height;
        _geometry.Opacity = FillOpacity;
        OverlayStore.Save(_geometry);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == FillOpacityProperty)
        {
            _fillBrush.Opacity = FillOpacity;
            QueueSave();
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (_geometry.HasPosition)
            Position = new PixelPoint(_geometry.X, _geometry.Y);
        _lastPosition = Position;
        _ready = true;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _saveDebounce.Stop();
        Persist();
        base.OnClosing(e);
    }

    private void OnHeaderPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private bool _resizing;
    private Point _resizeAnchor;
    private double _resizeStartWidth;
    private double _resizeStartHeight;

    private void OnResizePressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _resizing = true;
        _resizeAnchor = e.GetPosition(this);
        _resizeStartWidth = Bounds.Width;
        _resizeStartHeight = Bounds.Height;
        e.Pointer.Capture(sender as IInputElement);
        e.Handled = true;
    }

    private void OnResizeMoved(object? sender, PointerEventArgs e)
    {
        if (!_resizing)
            return;

        var current = e.GetPosition(this);
        Width = Math.Clamp(_resizeStartWidth + (current.X - _resizeAnchor.X), MinWidth, 2000);
        Height = Math.Clamp(_resizeStartHeight + (current.Y - _resizeAnchor.Y), MinHeight, 2000);
        e.Handled = true;
    }

    private void OnResizeReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_resizing)
            return;

        _resizing = false;
        e.Pointer.Capture(null);
        e.Handled = true;
        QueueSave();
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}
