using System.Windows.Controls;
using EWB_Tracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace EWB_Tracker.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<SettingsViewModel>();
            Loaded += async (s, e) =>
            {
                var viewModel = (SettingsViewModel)DataContext;
                await viewModel.InitializeAsync();
            };
        }
    }
}