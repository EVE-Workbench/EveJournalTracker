using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using SharedLibrary.Enums;
using SharedLibrary.Repositories.Interfaces;

namespace UI.avalonia.ViewModels;

public class DungeonViewModel : INotifyPropertyChanged
{
    public ObservableCollection<string> DungeonTypes { get; set; }
    public ObservableCollection<string> DungeonNames { get; set; } = [];
    public ObservableCollection<string> Factions { get; set; } = [];
    public ObservableCollection<string> Levels { get; set; } = [];

    private string _selectedDungeonType;
    private readonly IDungeonRepository dungeonRepository;

    public string SelectedDungeonType
    {
        get => _selectedDungeonType;
        set
        {
            _selectedDungeonType = value;
            OnPropertyChanged(nameof(SelectedDungeonType));
            OnPropertyChanged(nameof(IsCustomSelected));
            OnPropertyChanged(nameof(IsStandardSelected));


            if (_selectedDungeonType != "Custom")
            {
                // todo: create a dungeon service that handles the user input and returns a dictionary of possible field values based on the input.
                var dungeonType = Enum.Parse<DungeonType>(value);

                var dungeons = dungeonRepository.GetBaseQuery();
                var dungeonNames = dungeons.Where(d => d.Type == dungeonType).ToList();
                DungeonNames.Clear();
                foreach (var dungeonName in dungeonNames)
                {
                    DungeonNames.Add(dungeonName.Name);
                }
                OnPropertyChanged(nameof(DungeonNames));

            }
        }
    }

    public bool IsCustomSelected => SelectedDungeonType == "Custom";
    public bool IsStandardSelected => !string.IsNullOrEmpty(SelectedDungeonType) && SelectedDungeonType != "Custom";

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public DungeonViewModel(IDungeonRepository dungeonRepository)
    {
        this.dungeonRepository = dungeonRepository;

        DungeonTypes = new ObservableCollection<string>();
        foreach (var dungeonType in Enum.GetValues<DungeonType>())
        {
            DungeonTypes.Add(dungeonType.ToString());
        }
        DungeonTypes.Add("Custom");

    }
}
