using Avalonia.Controls;
using UI.avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace UI.avalonia.Views;

public partial class AccountView : UserControl
{
    public AccountView()
    {
        InitializeComponent();
        var viewModel = App.ServiceProvider.GetRequiredService<AccountViewModel>();
        DataContext = viewModel;
    }
}
