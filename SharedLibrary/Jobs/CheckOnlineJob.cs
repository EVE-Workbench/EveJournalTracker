using System.Diagnostics;
using System.Timers;
using SharedLibrary.Repositories;
using Timer = System.Timers.Timer;

namespace SharedLibrary.Jobs;

public class CheckOnlineJob
{
    private readonly Timer _timer;

    public CheckOnlineJob(double interval)
    {
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
        var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle) &&
                        p.MainWindowTitle.StartsWith("EVE -", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var characterRepository = CharacterRepository.Instance;
        var allCharacters = characterRepository.Characters.Values;

        var onlineCharacterNames = processes
            .Select(p => p.MainWindowTitle.Split('-')[1].Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var character in allCharacters)
        {
            character.Online = onlineCharacterNames.Contains(character.Name);
        }
    }
}