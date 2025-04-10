using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Models;
using SharedLibrary.Services;

namespace EWB_Tracker.Views;

public partial class DefaultView : UserControl
{
    private readonly FileWatcherService _fileWatcherService;

    public ObservableCollection<EveSystem> EveSystemCollection { get; set; } = new();

    public DefaultView()
    {
        InitializeComponent();

        _fileWatcherService = App.ServiceProvider.GetRequiredService<FileWatcherService>();
        _fileWatcherService.OnISKUpdated += OnNewLogEvent;

        DataContext = this;
    }

    private void OnNewLogEvent(object sender, (int TotalISK, int ISKChange) e)
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

}