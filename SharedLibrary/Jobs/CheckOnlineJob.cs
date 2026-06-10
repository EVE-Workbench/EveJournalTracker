using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Timers;
using SharedLibrary.Cache;
using SharedLibrary.Events;
using SharedLibrary.Services;
using Timer = System.Timers.Timer;

namespace SharedLibrary.Jobs;

public partial class CheckOnlineJob
{
    private readonly Timer _timer;
    private readonly CharacterCache _characterCache;
    private readonly CharacterService _characterService;
    private bool _isChecking = false;
    private static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public event EventHandler<CharacterStatusChangedEventArgs>? CharacterStatusChanged;

    [GeneratedRegex(@"/autoSelectCharacter:(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex AutoSelectCharacterRegex();

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

            HashSet<int> onlineCharacterIds;
            HashSet<string> onlineCharacterNames;

            if (IsLinux)
            {
                // On Linux (Wine/Proton), parse command line for /autoSelectCharacter:<id>
                onlineCharacterIds = GetOnlineCharacterIdsFromLinux();
                onlineCharacterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                // On Windows, use MainWindowTitle "EVE - CharacterName"
                onlineCharacterIds = new HashSet<int>();
                onlineCharacterNames = GetOnlineCharacterNamesFromWindows();
            }

            var allCharacters = _characterCache.GetAllCharacters();

            // check for each character in the cache
            foreach (var character in allCharacters)
            {
                var characterForUpdate = _characterCache.GetCharacter(character.CharacterId);
                if (characterForUpdate == null) continue;

                // Deactivated characters are not tracked at all.
                if (!characterForUpdate.Active) continue;

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
                bool isOnline;

                if (IsLinux)
                {
                    isOnline = onlineCharacterIds.Contains(character.CharacterId);
                }
                else
                {
                    isOnline = onlineCharacterNames.Contains(character.Name);
                }

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

    private static HashSet<string> GetOnlineCharacterNamesFromWindows()
    {
        var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle) &&
                        p.MainWindowTitle.StartsWith("EVE -", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return processes
            .Select(p => p.MainWindowTitle.Split('-')[1].Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static HashSet<int> GetOnlineCharacterIdsFromLinux()
    {
        var characterIds = new HashSet<int>();

        try
        {
            // Read all process command lines from /proc
            var procDirs = Directory.GetDirectories("/proc")
                .Where(d => int.TryParse(Path.GetFileName(d), out _));

            foreach (var procDir in procDirs)
            {
                try
                {
                    var cmdlinePath = Path.Combine(procDir, "cmdline");
                    if (!File.Exists(cmdlinePath)) continue;

                    var cmdline = File.ReadAllText(cmdlinePath);

                    // Check if this is an EVE process (exefile.exe)
                    if (!cmdline.Contains("exefile.exe", StringComparison.OrdinalIgnoreCase)) continue;

                    // Extract character ID from /autoSelectCharacter:<id>
                    var match = AutoSelectCharacterRegex().Match(cmdline);
                    if (match.Success && int.TryParse(match.Groups[1].Value, out var characterId))
                    {
                        characterIds.Add(characterId);
                    }
                }
                catch
                {
                    // Skip processes we can't read (permission denied, process exited, etc.)
                }
            }
        }
        catch
        {
            // If we can't read /proc at all, return empty set
        }

        return characterIds;
    }
}