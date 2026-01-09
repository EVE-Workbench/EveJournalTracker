using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using UI.avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace UI.avalonia.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        var viewModel = App.ServiceProvider.GetRequiredService<SettingsViewModel>();
        DataContext = viewModel;
        _ = viewModel.InitializeAsync();
    }

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
