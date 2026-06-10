using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using UI.avalonia.ViewModels;

namespace UI.avalonia.Views;

public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode && App.ServiceProvider is { } serviceProvider)
            DataContext = serviceProvider.GetRequiredService<LogViewModel>();
    }
}
