using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using SharedLibrary.Cache;
using SharedLibrary.Data;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using SharedLibrary.Models.Database;

namespace SharedLibrary.Services;

public class FileWatcherService : IDisposable
{
    private readonly string _logDirectory;
    private readonly CharacterCache _characterCache;
    private readonly CharacterService _characterService;
    private readonly AppDbContext _context;
    private readonly List<LogEvent> _logEvents;
    private readonly List<FileSystemWatcher> _fileWatchers;
    private readonly ConcurrentQueue<string> _fileChangeQueue;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Dictionary<string, long> _filePositions = new();

    public event EventHandler<LogEvent>? OnNewLogEvent;
    public event EventHandler<(int TotalISK, int ISKChange, Character Character)>? OnISKUpdated;

    private List<EveSystemDto> EveSystems { get; set; } = [];
    private List<EveSystem> EveSystemsList { get; set; } = [];

    public FileWatcherService(string logDirectory, CharacterCache characterCache, CharacterService characterService, AppDbContext context)
    {
        _logDirectory = logDirectory;
        _characterCache = characterCache;
        _characterService = characterService;
        _context = context;
        _logEvents = [];
        _fileWatchers = [];
        _fileChangeQueue = new ConcurrentQueue<string>();
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => ProcessFileChanges(_cancellationTokenSource.Token));
    }

    public void StartWatching()
    {   
        EveSystems = _context.EveSystems.ToList();
        
        // check if the log directory exists
        if (!Directory.Exists(_logDirectory))
        {
            Console.WriteLine("Log directory doesn't exist");
            return;
            //throw new DirectoryNotFoundException($"The log directory '{_logDirectory}' does not exist.");
        }
        
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
        foreach (var watcher in _fileWatchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        _fileWatchers.Clear();
    }

    public void Dispose()
    {
        StopWatching();

        if (!_cancellationTokenSource.IsCancellationRequested)
            _cancellationTokenSource.Cancel();

        _cancellationTokenSource.Dispose();
    }

    private async Task ProcessFileChanges(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (_fileChangeQueue.TryDequeue(out var filePath))
                {
                    try
                    {
                        _filePositions.TryAdd(filePath, 0);

                        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        stream.Seek(_filePositions[filePath], SeekOrigin.Begin);

                        using var reader = new StreamReader(stream);
                        while (await reader.ReadLineAsync(token) is { } line)
                        {
                            var characterId = Convert.ToInt32(ExtractCharacterIdFromFileName(Path.GetFileName(filePath)));
                            var character = _characterService.GetOrCreateCharacter(characterId);

                            var logEvent = ParseLogLine(line, character);

                            // Ignore empty log events
                            if (logEvent == null) continue;

                            _logEvents.Add(logEvent);

                            OnNewLogEvent?.Invoke(this, logEvent);
                            // Update ISK values
                            var totalIsk = _logEvents.Sum(log => log.BountyValue) ?? 0;
                            var lastBountyEvent = _logEvents.LastOrDefault(log => log.Type == LogEventType.Bounty);
                            var iskChange = lastBountyEvent?.BountyValue ?? 0;
                            if (logEvent.Type == LogEventType.Bounty)
                            {
                                OnISKUpdated?.Invoke(this, (totalIsk, iskChange, character));
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
                    await Task.Delay(100, token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the service is disposed during shutdown.
        }
    }


    private static string? ExtractCharacterIdFromFileName(string fileName)
    {
        // Format: YYYYMMDD_HHMMSS_CharacterId.log
        var parts = fileName.Split('_');
        return parts.Length >= 3 ? Path.GetFileNameWithoutExtension(parts[2]) : null;
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
        const string timePattern = @"\[ ([\d\.: ]+) \]";
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

                character.EveSystem = EveSystems.FirstOrDefault(x => x.Name == to);

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
                }
            }

            logEvent.Type = LogEventType.Bounty;
            //logEvent.Value = line;
            
            UpdateEveSystemsList(logEvent);
            
            return logEvent;
        }

        if (line.Contains("(combat)"))
        {
            var damageTakenRegex = new Regex(@"<b>(\d+)</b>.*?<font size=10>from</font>.*?<b><color=.*?>(.*?)</b><font size=10>.*?(?: - (.*?))? - (Grazes|Glances Off|Hits|Penetrates|Smashes|Wrecks)");
            var damageDoneRegex = new Regex(@"<b>(\d+)</b> <color=.*>to</font> <b><color=.*>(.*?)</b><font size=\d+>.*? - (.*?) - (Grazes|Glances Off|Hits|Penetrates|Smashes|Wrecks)");
            var damageOutMissRegex = new Regex(@"Your (.*?) misses (.*?) completely - (.*?)");
            var damageInMissRegex = new Regex(@"misses you completely");
            var capOutRegex = new Regex(@"<b>(\d+)</b><color=0x77ffffff><font size=10> remote capacitor transmitted to");
            var neutInRegex = new Regex(@"<color=0xffe57f7f><b>(\d+)\sGJ</b><color=0x77ffffff><font size=10> energy neutralized ");
            var neutOutRegex = new Regex(@"<color=0xff7fffff><b>(\d+)\sGJ</b><color=0x77ffffff><font size=10> energy neutralized ");
            var repOutRegex = new Regex(@"<b>(\d+)</b><color=0x77ffffff><font size=10> remote (?:armor repaired|shield boosted) to");
            
            logEvent.Type = LogEventType.Combat;
            
            
            if (damageTakenRegex.IsMatch(line))
            {
                var match = damageTakenRegex.Match(line);
                if (!match.Success) return logEvent;
                logEvent.DamageType = "DpsIn";
                logEvent.DamageQuality = match.Groups[4].Value;
                logEvent.Value = match.Groups[1].Value;
                return logEvent;
            }
            
            if (damageDoneRegex.IsMatch(line))
            {
                var match = damageDoneRegex.Match(line);
                if (!match.Success) return logEvent;
                logEvent.DamageType = "DpsOut";
                logEvent.DamageQuality = match.Groups[4].Value;
                logEvent.Value = match.Groups[1].Value;
                return logEvent;
            }
            
            if (damageOutMissRegex.IsMatch(line))
            {
                var match = damageOutMissRegex.Match(line);
                if (!match.Success) return logEvent;
                logEvent.DamageType = "DpsOutMiss";
                logEvent.DamageQuality = "Miss";
                return logEvent;
            }
            
            if (damageInMissRegex.IsMatch(line))
            {
                var match = damageInMissRegex.Match(line);
                if (!match.Success) return logEvent;
                logEvent.DamageType = "DpsInMiss";
                logEvent.DamageQuality = "Miss";
                return logEvent;
            }
            
            if (capOutRegex.IsMatch(line))
            {
                var match = capOutRegex.Match(line);
                if (!match.Success) return logEvent;
                logEvent.DamageType = "CapOut";
                return logEvent;
            }

            logEvent.Value = line;
            return logEvent;
        }

        return null;
    }

    private void UpdateEveSystemsList(LogEvent logEvent)
    {
        // ignore if logEvent.EveSystem is null
        if (logEvent.EveSystem == null)
        {
            return;
        }
        
        var eveSystem = EveSystemsList.FirstOrDefault(x => x.EveSystemDto == logEvent.EveSystem);
        if (eveSystem != null)
        {
            eveSystem.Bounty += logEvent.BountyValue ?? 0;
            eveSystem.LastUpdated = logEvent.Timestamp;
        }
        else
        {
            EveSystemsList.Add(new EveSystem
            {
                EveSystemDto = logEvent.EveSystem,
                Bounty = logEvent.BountyValue ?? 0,
                LastUpdated = logEvent.Timestamp
            });
        }
    }

    public List<LogEvent> GetLogEvents()
    {
        return _logEvents;
    }
    
    public List<EveSystem> GetEveSystemsList()
    {
        EveSystemsList = EveSystemsList.OrderByDescending(x => x.LastUpdated).ToList();
        
        return EveSystemsList;
    }
}