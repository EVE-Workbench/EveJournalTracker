using Avalonia.Controls;
using Avalonia.Interactivity;
using UI.avalonia.ViewModels;

namespace UI.avalonia.Views;

public partial class BountyRunView : UserControl
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
            var parentWindow = this.FindAncestorOfType<MainWindow>();
            parentWindow?.CloseModal();
        };

        // Focus the textbox when view loads
        Loaded += (s, e) => RunNameTextBox.Focus();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        var parentWindow = this.FindAncestorOfType<MainWindow>();
        parentWindow?.CloseModal();
    }
}

public static class VisualExtensions
{
    public static T FindAncestorOfType<T>(this Control control) where T : class
    {
        var parent = control.Parent;
        while (parent != null)
        {
            if (parent is T result)
                return result;
            if (parent is Control parentControl)
                parent = parentControl.Parent;
            else
                break;
        }
        return null;
    }
}
