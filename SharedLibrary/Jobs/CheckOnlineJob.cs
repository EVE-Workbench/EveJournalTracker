using System.Diagnostics;
using System.Timers;
using SharedLibrary.Cache;
using Timer = System.Timers.Timer;

namespace SharedLibrary.Jobs;

public class CheckOnlineJob
{
    private readonly Timer _timer;
    private readonly CharacterCache _characterCache;

    public CheckOnlineJob(double interval, CharacterCache characterCache)
    {
        _characterCache = characterCache ?? throw new ArgumentNullException(nameof(characterCache));
        _timer = new Timer(interval);
        _timer.Elapsed += CheckProcesses;
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
        // Haal alle actieve EVE Online processen op
        var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle) &&
                        p.MainWindowTitle.StartsWith("EVE -", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Verkrijg alle characters uit de cache
        var allCharacters = _characterCache.GetAllCharacters();

        // Alle online character namen
        HashSet<string?> onlineCharacterNames = processes
            .Select(p => p.MainWindowTitle.Split('-')[1].Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Controleer voor elke character of deze online is
        foreach (var character in allCharacters)
        {
            var characterForUpdate = _characterCache.GetCharacter(character.CharacterId);
            if(characterForUpdate != null)
            {
                characterForUpdate.Online = onlineCharacterNames.Contains(character.Name);
            }
        }
    }
}