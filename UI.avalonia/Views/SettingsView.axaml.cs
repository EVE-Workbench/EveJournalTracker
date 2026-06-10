using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using UI.avalonia.Input;
using UI.avalonia.Services;
using UI.avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace UI.avalonia.Views;

public partial class SettingsView : UserControl
{
    private readonly SettingsViewModel _viewModel;

    public SettingsView()
    {
        InitializeComponent();
        _viewModel = App.ServiceProvider.GetRequiredService<SettingsViewModel>();
        DataContext = _viewModel;
        _ = _viewModel.InitializeAsync();

        Focusable = true;
        // Tunnel so a shortcut being recorded is captured before the main window acts on it.
        AddHandler(KeyDownEvent, OnCaptureKeyDown, RoutingStrategies.Tunnel);
        AddHandler(PointerPressedEvent, OnCapturePointer, RoutingStrategies.Tunnel);
    }

    private void OnRecordShortcut(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: string commandId })
        {
            _viewModel.BeginRecording(commandId);
            Focus();
        }
    }

    private void OnClearShortcut(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: string commandId })
            _viewModel.ClearBinding(commandId);
    }

    private void OnCaptureKeyDown(object? sender, KeyEventArgs e)
    {
        if (!_viewModel.IsRecording)
            return;

        if (e.Key == Key.Escape)
        {
            _viewModel.CancelRecording();
            e.Handled = true;
            return;
        }

        if (IsModifierKey(e.Key))
            return; // wait for a non-modifier key

        _viewModel.ApplyRecording(new ShortcutGesture { Modifiers = e.KeyModifiers, Key = e.Key });
        e.Handled = true;
    }

    private void OnCapturePointer(object? sender, PointerPressedEventArgs e)
    {
        if (!_viewModel.IsRecording)
            return;

        var button = ShortcutService.ToMouseButton(e.GetCurrentPoint(this).Properties.PointerUpdateKind);
        if (button == ShortcutMouseButton.None)
            return; // left/right click: keep waiting

        _viewModel.ApplyRecording(new ShortcutGesture { Modifiers = e.KeyModifiers, MouseButton = button });
        e.Handled = true;
    }

    private static bool IsModifierKey(Key key) => key is
        Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or
        Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin;

    private void OnAccessTokenLinkPressed(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://evejournal.com/my-account/personal-access-tokens",
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail if browser cannot be opened
        }
    }
}
