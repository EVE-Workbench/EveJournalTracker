using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using SharedLibrary.Repositories;

namespace SharedLibrary.Services;

public class FileWatcherService
{
    private readonly string _logDirectory;
    private readonly List<LogEvent> _logEvents;
    private readonly List<FileSystemWatcher> _fileWatchers;
    private readonly ConcurrentQueue<string> _fileChangeQueue;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Dictionary<string, long> _filePositions = new();

    public event EventHandler<LogEvent> OnNewLogEvent;
    public event EventHandler<(int TotalISK, int ISKChange)> OnISKUpdated;

    public FileWatcherService(string logDirectory)
    {
        _logDirectory = logDirectory;
        _logEvents = new List<LogEvent>();
        _fileWatchers = new List<FileSystemWatcher>();
        _fileChangeQueue = new ConcurrentQueue<string>();
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => ProcessFileChanges(_cancellationTokenSource.Token));
    }

    public void StartWatching()
    {
        var watcher = new FileSystemWatcher
        {
            Path = _logDirectory,
            Filter = "*_*_*.txt",
            NotifyFilter = NotifyFilters.Attributes
                           | NotifyFilters.CreationTime
                           | NotifyFilters.DirectoryName
                           | NotifyFilters.FileName
                           | NotifyFilters.LastAccess
                           | NotifyFilters.LastWrite
                           | NotifyFilters.Security
                           | NotifyFilters.Size,
            IncludeSubdirectories = false
        };

        watcher.Changed += (sender, e) => _fileChangeQueue.Enqueue(e.FullPath);
        watcher.Created += (sender, e) => _fileChangeQueue.Enqueue(e.FullPath);
        watcher.EnableRaisingEvents = true;

        _fileWatchers.Add(watcher);
    }

    public void StopWatching()
    {
        _cancellationTokenSource.Cancel();
        foreach (var watcher in _fileWatchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        _fileWatchers.Clear();
    }

    private async Task ProcessFileChanges(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_fileChangeQueue.TryDequeue(out var filePath))
            {
                try
                {
                    if (!_filePositions.ContainsKey(filePath))
                    {
                        _filePositions[filePath] = 0;
                    }

                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    stream.Seek(_filePositions[filePath], SeekOrigin.Begin);

                    using var reader = new StreamReader(stream);
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var characterId = Convert.ToInt32(ExtractCharacterIdFromFileName(Path.GetFileName(filePath)));
                        var character = GetOrCreateCharacter(characterId);

                        var logEvent = ParseLogLine(line, character);

                        if (logEvent != null)
                        {
                            _logEvents.Add(logEvent);

                            OnNewLogEvent?.Invoke(this, logEvent);
                            // Update ISK values
                            var totalIsk = _logEvents.Sum(log => log.BountyValue) ?? 0;
                            var lastBountyEvent = _logEvents.LastOrDefault(log => log.Type == LogEventType.Bounty);
                            var iskChange = lastBountyEvent?.BountyValue ?? 0;
                            OnISKUpdated?.Invoke(this, (totalIsk, iskChange));
                        }
                    }

                    _filePositions[filePath] = stream.Position;
                }
                catch (IOException)
                {
                    // Ignore for now
                }
            }
            else
            {
                await Task.Delay(100);
            }
        }
    }

    private void OnLogFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            var lines = File.ReadAllLines(e.FullPath);
            var lastLine = lines[^1];
            var characterId = Convert.ToInt32(ExtractCharacterIdFromFileName(e.Name!));
            var character = GetOrCreateCharacter(characterId);

            var logEvent = ParseLogLine(lastLine, character);

            if (logEvent != null)
            {
                _logEvents.Add(logEvent);

                OnNewLogEvent?.Invoke(this, logEvent);
                // Update ISK values
                var totalIsk = _logEvents.Sum(log => log.BountyValue) ?? 0;
                var lastBountyEvent = _logEvents.LastOrDefault(log => log.Type == LogEventType.Bounty);
                var iskChange = lastBountyEvent?.BountyValue ?? 0;
                OnISKUpdated?.Invoke(this, (totalIsk, iskChange));
            }
        }
        catch (IOException)
        {
            // Ignore for now
        }
    }


    private string? ExtractCharacterIdFromFileName(string fileName)
    {
        // Format: YYYYMMDD_HHMMSS_CharacterId.log
        var parts = fileName.Split('_');
        return parts.Length >= 3 ? Path.GetFileNameWithoutExtension(parts[2]) : null;
    }

    private Character GetOrCreateCharacter(int characterId)
    {
        return CharacterRepository.Instance.GetOrCreateCharacter(characterId, id =>
        {
            var characterName = CharacterService.GetCharacterNameAsync(id).GetAwaiter().GetResult();
            return new Character
            {
                CharacterId = id,
                Name = characterName ?? $"Char-{id}"
            };
        });
    }

    private LogEvent? ParseLogLine(string line, Character character)
    {
        var logEvent = new LogEvent()
        {
            Character = character,
            EveSystem = character.EveSystem,
            Raw = line,
        };

        // get the system time for the log line
        var timePattern = @"\[ ([\d\.: ]+) \]";
        var timeMatch = Regex.Match(line, timePattern);
        if (timeMatch.Success)
        {
            var timeString = timeMatch.Groups[1].Value;
            if (DateTime.TryParse(timeString, out var time))
            {
                logEvent.Timestamp = time;
            }
        }

        if (line.Contains("Jumping from"))
        {
            const string jumpPattern = @"Jumping from (.+) to (.+)";
            var match = Regex.Match(line, jumpPattern);
            if (match.Success)
            {
                var from = match.Groups[1].Value;
                var to = match.Groups[2].Value;

                character.EveSystem = new EveSystem { Name = to };

                logEvent.Type = LogEventType.Jump;
                logEvent.Value = $"{from} > {to}";
                return logEvent;
            }
        }

        if (line.Contains("(bounty)"))
        {
            const string bountyPattern = @"<color=0xff00aa00>([\d,. ]+) ISK</b>";
            var match = Regex.Match(line, bountyPattern);

            if (match.Success)
            {
                var valueString = match.Groups[1].Value.Replace(",", "").Replace(".", "").Replace(" ", "");
                if (int.TryParse(valueString, out var value))
                {
                    logEvent.BountyValue = value;

                    var totalCharacterBounty = _logEvents
                        .Where(log =>
                            log.Character.CharacterId == character.CharacterId && log.Type == LogEventType.Bounty)
                        .Sum(log => log.BountyValue);

                    character.Bounty = totalCharacterBounty + value ?? value;
                    return logEvent;
                }
            }

            logEvent.Type = LogEventType.Bounty;
            logEvent.Value = line;
            return logEvent;
        }

        if (line.Contains("(combat)"))
        {
            logEvent.Type = LogEventType.Combat;
            return logEvent;
        }

        return null;
    }

    public List<LogEvent> GetLogEvents()
    {
        return _logEvents;
    }
}