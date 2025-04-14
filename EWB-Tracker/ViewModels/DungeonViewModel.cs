using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SharedLibrary.Enums;

namespace EWB_Tracker.ViewModels;

public class DungeonViewModel : INotifyPropertyChanged
{
    public ObservableCollection<string> DungeonTypes { get; set; }
    public ObservableCollection<string> DungeonNames { get; set; } = new() { "Outpost", "Stronghold", "Refinery" };
    public ObservableCollection<string> Factions { get; set; } = new() { "Sansha", "Blood Raiders", "Guristas" };
    public ObservableCollection<string> Levels { get; set; } = new() { "1", "2", "3", "4", "5" };

    private string _selectedDungeonType;
    public string SelectedDungeonType
    {
        get => _selectedDungeonType;
        set
        {
            _selectedDungeonType = value;
            OnPropertyChanged(nameof(SelectedDungeonType));
            OnPropertyChanged(nameof(IsCustomSelected));
            OnPropertyChanged(nameof(IsStandardSelected));
        }
    }

    public bool IsCustomSelected => SelectedDungeonType == "Custom";
    public bool IsStandardSelected => !string.IsNullOrEmpty(SelectedDungeonType) && SelectedDungeonType != "Custom";

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public DungeonViewModel()
    {
        DungeonTypes = new ObservableCollection<string>();
        foreach (var dungeonType in Enum.GetValues<DungeonType>())
        {
            DungeonTypes.Add(dungeonType.ToString());
        }
        DungeonTypes.Add("Custom");
    }
}