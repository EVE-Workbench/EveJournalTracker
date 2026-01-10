using System.Diagnostics;
using System.Timers;
using SharedLibrary.Cache;
using SharedLibrary.Events;
using SharedLibrary.Services;
using Timer = System.Timers.Timer;

namespace SharedLibrary.Jobs;

public class CheckOnlineJob
{
    private readonly Timer _timer;
    private readonly CharacterCache _characterCache;
    private readonly CharacterService _characterService;
    private bool _isChecking = false;
    public event EventHandler<CharacterStatusChangedEventArgs>? CharacterStatusChanged;

    public CheckOnlineJob(double interval, CharacterCache characterCache, CharacterService characterService)
    {
        _characterCache = characterCache ?? throw new ArgumentNullException(nameof(characterCache));
        _characterService = characterService;
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
        // Prevent re-entrant calls - if already checking, skip this run
        if (_isChecking)
        {
            return;
        }

        try
        {
            _isChecking = true;

            // Fetch all processes with a main window title that starts with "EVE -"
            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle) &&
                            p.MainWindowTitle.StartsWith("EVE -", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var onlineCharacterNames = processes
                .Select(p => p.MainWindowTitle.Split('-')[1].Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var allCharacters = _characterCache.GetAllCharacters();

            // check for each character in the cache
            foreach (var character in allCharacters)
            {
                var characterForUpdate = _characterCache.GetCharacter(character.CharacterId);
                if (characterForUpdate == null) continue;

                // Retry fetching character name if it's still using the fallback name
                if (characterForUpdate.Name.StartsWith("Char-") &&
                    int.TryParse(characterForUpdate.Name.Substring(5), out var charIdFromName) &&
                    charIdFromName == characterForUpdate.CharacterId)
                {
                    try
                    {
                        var realName = _characterService.GetCharacterNameAsync(characterForUpdate.CharacterId).GetAwaiter().GetResult();
                        if (!string.IsNullOrEmpty(realName))
                        {
                            characterForUpdate.Name = realName;
                        }
                    }
                    catch
                    {
                        // Silently continue if name fetch fails - will retry next time
                    }
                }

                var wasOnline = characterForUpdate.Online;
                var isOnline = onlineCharacterNames.Contains(character.Name);

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
        finally
        {
            _isChecking = false;
        }
    }
}