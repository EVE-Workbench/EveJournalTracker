using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EWB_Tracker.Commands;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using SharedLibrary.Services;

namespace EWB_Tracker.Views;

public partial class LogView
{
    
    private readonly FileWatcherService _fileWatcherService;
    private ObservableCollection<LogEvent> LogEventCollection { get; set; }

    public ICommand ImportLogEventsCommand { get; }
    public ICommand ClearLogEventsCommand { get; }
    public ICommand SortLogEventsCommand { get; }

    public LogView()
    {
        InitializeComponent();
        LogEventCollection = new ObservableCollection<LogEvent>();
        LogListBox.ItemsSource = LogEventCollection;
        
        _fileWatcherService = App.ServiceProvider.GetRequiredService<FileWatcherService>();

        _fileWatcherService.OnNewLogEvent += OnNewLogEvent;

        ImportLogEventsCommand = new RelayCommand(ImportLogEvents);
        ClearLogEventsCommand = new RelayCommand(ClearLogEvents);
        SortLogEventsCommand = new RelayCommand(SortLogEvents);
    }

    // Event handler for new log events
    private void OnNewLogEvent(object sender, LogEvent e)
    {
        Dispatcher.Invoke(() =>
        {
            //if (e.Type == LogEventType.Combat) return;
            
            LogEventCollection.Add(e);     
            
            // only keep the last xxx log events to keep things fast
            if (LogEventCollection.Count > 250)
            {
                LogEventCollection.RemoveAt(0);
            }
            
            //SortLogEvents();
            LogListBox.ScrollIntoView(e);
        });
    }

    private void ImportLogEvents()
    {
        var fileDialog = new Microsoft.Win32.OpenFileDialog
        {
            DefaultExt = ".txt",
            Filter = "Text documents (.txt)|*.txt"
        };

        var result = fileDialog.ShowDialog();
        if (result == true)
        {
            var filePath = fileDialog.FileName;
            // Implement import logic 
        }
    }

    private void ClearLogEvents()
    {
        LogEventCollection.Clear();
    }

    private void SortLogEvents()
    {
        var sorted = LogEventCollection.OrderBy(log => log.Timestamp).ToList();
        LogEventCollection.Clear();
        foreach (var logEvent in sorted)
        {
            LogEventCollection.Add(logEvent);
        }
        sorted.Clear();
    }

}