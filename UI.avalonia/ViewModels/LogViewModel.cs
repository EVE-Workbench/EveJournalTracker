using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Threading;
using SharedLibrary.Models;
using SharedLibrary.Services;
using UI.avalonia.Commands;

namespace UI.avalonia.ViewModels;

public class LogViewModel
{
    private const int MaxItems = 500;

    private readonly FileWatcherService _fileWatcherService;
    private bool _newestFirst = true;

    public ObservableCollection<LogEvent> LogEvents { get; } = new();

    public ICommand ImportLogEventsCommand { get; }
    public ICommand ClearLogEventsCommand { get; }
    public ICommand SortLogEventsCommand { get; }

    public LogViewModel(FileWatcherService fileWatcherService)
    {
        _fileWatcherService = fileWatcherService;

        ImportLogEventsCommand = new RelayCommand(LoadEntireSession);
        ClearLogEventsCommand = new RelayCommand(LogEvents.Clear);
        SortLogEventsCommand = new RelayCommand(ToggleSort);

        _fileWatcherService.OnNewLogEvent += OnNewLogEvent;
    }

    private void OnNewLogEvent(object? sender, LogEvent logEvent)
    {
        // The live tail only shows events as they happen; replaying the whole backfilled
        // session here would flood the UI thread at startup. Use Import for the full session.
        if (logEvent.IsHistorical)
            return;

        Dispatcher.UIThread.Post(() => Add(logEvent));
    }

    private void Add(LogEvent logEvent)
    {
        LogEvents.Insert(_newestFirst ? 0 : LogEvents.Count, logEvent);

        while (LogEvents.Count > MaxItems)
            LogEvents.RemoveAt(_newestFirst ? LogEvents.Count - 1 : 0);
    }

    private void LoadEntireSession()
    {
        var events = _fileWatcherService.GetLogEvents();
        Replace(Sort(events).Take(MaxItems));
    }

    private void ToggleSort()
    {
        _newestFirst = !_newestFirst;
        Replace(Sort(LogEvents));
    }

    private IEnumerable<LogEvent> Sort(IEnumerable<LogEvent> source)
        => _newestFirst
            ? source.OrderByDescending(e => e.Timestamp)
            : source.OrderBy(e => e.Timestamp);

    private void Replace(IEnumerable<LogEvent> events)
    {
        var ordered = events.ToList();
        LogEvents.Clear();
        foreach (var logEvent in ordered)
            LogEvents.Add(logEvent);
    }
}
