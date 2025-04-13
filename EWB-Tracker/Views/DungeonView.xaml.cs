using System.Windows.Controls;
using EWB_Tracker.ViewModels;

namespace EWB_Tracker.Views;

public partial class DungeonView
{
    public DungeonView()
    {
        InitializeComponent();
        DataContext = new DungeonViewModel();
    }
}