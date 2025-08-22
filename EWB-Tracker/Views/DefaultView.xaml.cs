using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using EWB_Tracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Models;
using SharedLibrary.Services;

namespace EWB_Tracker.Views;

public partial class DefaultView : UserControl, INotifyPropertyChanged
{
    private readonly FileWatcherService _fileWatcherService;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public ObservableCollection<EveSystem> EveSystemCollection { get; set; } = new();
    public ObservableCollection<BountyRun> BountyRuns => _mainWindowViewModel.BountyRuns;
    public ObservableCollection<BountyRun> TopBountyRuns => _mainWindowViewModel.TopBountyRuns;

    public DefaultView()
    {
        InitializeComponent();

        _fileWatcherService = App.ServiceProvider.GetRequiredService<FileWatcherService>();
        _mainWindowViewModel = App.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        
        _mainWindowViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        _fileWatcherService.OnISKUpdated += OnNewLogEvent;

        DataContext = this;
    }
    
    private void OnMainViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_mainWindowViewModel.TopBountyRuns))
        {
            OnPropertyChanged(nameof(TopBountyRuns));
        }
    }

    private void OnNewLogEvent(object sender, (int TotalISK, int ISKChange, Character Character) e)
    {
        Dispatcher.Invoke(() =>
        {
            var newSystems = _fileWatcherService.GetEveSystemsList();

            foreach (var newSystem in newSystems)
            {
                var existingSystem = EveSystemCollection.FirstOrDefault(
                    s => s.EveSystemDto.Name == newSystem.EveSystemDto.Name
                );

                if (existingSystem != null)
                {
                    // Update existing system
                    existingSystem.Bounty = newSystem.Bounty;
                    existingSystem.LastUpdated = newSystem.LastUpdated;
                }
                else
                {
                    // Add the system, it's not in the list
                    EveSystemCollection.Add(newSystem);
                }
            }

            // Sort by lastUpdated field
            var sorted = EveSystemCollection
                .OrderByDescending(s => s.LastUpdated)
                .ToList();

            // Reorder the collection
            for (int i = 0; i < sorted.Count; i++)
            {
                var item = sorted[i];
                var currentIndex = EveSystemCollection.IndexOf(item);
                if (currentIndex != i)
                {
                    EveSystemCollection.Move(currentIndex, i);
                }
            }
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}