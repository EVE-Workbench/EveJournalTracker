using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
                //SortLogEvents();
                LogListBox.ScrollIntoView(e);
            }
        });
    }
    
    
    private void SortLogEvents_Click(object sender, RoutedEventArgs e)
    {
        var sorted = _logEventCollection.OrderBy(log => log.Timestamp).ToList();
        _logEventCollection.Clear();
        foreach (var logEvent in sorted)
        {
            _logEventCollection.Add(logEvent);
        }
    }
    
    private void ClearLogEvents_Click(object sender, RoutedEventArgs e)
    {
        _logEventCollection.Clear();
    }
    
    private void ImportLogEvents_Click(object sender, RoutedEventArgs e)
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
            
            // for now this uses the file touch event to trigger the file processing because the files in the evelog directory are being watched. 
            // this needs to be replaced by an actual import function that reads the file and processes the logs. but leaves the current user system untouched.
        }
    }
}