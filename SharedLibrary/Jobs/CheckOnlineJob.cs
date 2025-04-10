using System.Diagnostics;
using System.Timers;
using SharedLibrary.Cache;
using SharedLibrary.Events;
using Timer = System.Timers.Timer;

namespace SharedLibrary.Jobs;

public class CheckOnlineJob
{
    private readonly Timer _timer;
    private readonly CharacterCache _characterCache;
    public event EventHandler<CharacterStatusChangedEventArgs>? CharacterStatusChanged;
    
    public CheckOnlineJob(double interval, CharacterCache characterCache)
    {
        _characterCache = characterCache ?? throw new ArgumentNullException(nameof(characterCache));
        _timer = new Timer(interval);
        _timer.Elapsed += CheckProcesses!;
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    private void CheckProcesses(object sender, ElapsedEventArgs e)
    {
        // Fetch all processes with a main window title that starts with "EVE -"
        var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle) &&
                        p.MainWindowTitle.StartsWith("EVE -", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Get all characters from the cache
        var allCharacters = _characterCache.GetAllCharacters();

        var onlineCharacterNames = processes
            .Select(p => p.MainWindowTitle.Split('-')[1].Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // check for each character in the cache
        foreach (var character in allCharacters)
        {
            var characterForUpdate = _characterCache.GetCharacter(character.CharacterId);
            if (characterForUpdate != null)
            {
                bool wasOnline = characterForUpdate.Online;
                bool isOnline = onlineCharacterNames.Contains(character.Name);

                if (wasOnline != isOnline)
                {
                    characterForUpdate.Online = isOnline;

                    // Fire the event
                    CharacterStatusChanged?.Invoke(this, new CharacterStatusChangedEventArgs(
                        character.CharacterId,
                        character.Name,
                        isOnline
                    ));
                }
            }
        }
    }
}