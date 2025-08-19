using System.Windows;
using EWB_Tracker.ViewModels;

namespace EWB_Tracker.Views;

public partial class BountyRunView
{
    private readonly BountyRunViewModel _viewModel;

    public BountyRunView(BountyRunViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        // Handle run started event to close modal
        _viewModel.OnRunStarted += () =>
        {
            // Find parent window and close modal
            var parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow?.CloseModal();
        };
        
        // Focus the textbox when view loads
        Loaded += (s, e) => RunNameTextBox.Focus();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        var parentWindow = Window.GetWindow(this) as MainWindow;
        parentWindow?.CloseModal();
    }
}