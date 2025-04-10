using System.Collections.ObjectModel;
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
            var eveSystems = _fileWatcherService.GetEveSystemsList();

            EveSystemCollection.Clear();
            foreach (var system in eveSystems)
            {
                EveSystemCollection.Add(system);
            }
        });
    }
}