using Avalonia.Controls;
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
        // TODO: Port full SettingsView logic from WPF project
    }
}
