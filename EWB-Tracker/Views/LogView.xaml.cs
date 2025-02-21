using System.Collections.ObjectModel;
using System.Windows.Controls;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using SharedLibrary.Services;

namespace EWB_Tracker.Views;

public partial class LogView : UserControl
{
    private FileWatcherService _fileWatcherService;
    private ObservableCollection<LogEvent> _logEventCollection;
    
    public LogView()
    {
        InitializeComponent();
        _fileWatcherService = ServiceLocator.GetService<FileWatcherService>();
        _logEventCollection = new ObservableCollection<LogEvent>();
        LogListBox.ItemsSource = _logEventCollection;
        
        // Subscribe op het event voor nieuwe log entries
        _fileWatcherService.OnNewLogEvent += OnNewLogEvent;
    }
    

    // Event handler voor nieuwe log entries
    private void OnNewLogEvent(object sender, LogEvent e)
    {
        // UI updaten moet op de UI thread gebeuren
        Dispatcher.Invoke(() =>
        {
            if (e.Type != LogEventType.Combat)
            {
                _logEventCollection.Add(e);
                LogListBox.ScrollIntoView(e);
            }
        });
    }
}