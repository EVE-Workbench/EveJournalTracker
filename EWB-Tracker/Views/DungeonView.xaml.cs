using EWB_Tracker.ViewModels;

namespace EWB_Tracker.Views;

public partial class DungeonView
{
    public DungeonView(DungeonViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}