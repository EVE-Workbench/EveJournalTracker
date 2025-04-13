using System;
using System.Collections.ObjectModel;
using SharedLibrary.Enums;

namespace EWB_Tracker.ViewModels;

public class DungeonViewModel
{
    public ObservableCollection<string> DungeonTypes { get; set; }
    public string SelectedDungeonType { get; set; }

    public DungeonViewModel()
    {
        DungeonTypes = new ObservableCollection<string>();
        foreach (var dungeonType in Enum.GetValues<DungeonType>())
        {
            DungeonTypes.Add(dungeonType.ToString());
        }
    }
}