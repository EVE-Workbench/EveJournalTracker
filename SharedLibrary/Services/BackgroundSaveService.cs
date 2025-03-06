using System.Windows;
using Microsoft.Extensions.Hosting;
using SharedLibrary.Cache;

namespace SharedLibrary.Services;

public class BackgroundSaveService : BackgroundService
{
    private readonly CharacterCache _characterCache;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public BackgroundSaveService(CharacterCache characterCache)
    {
        ArgumentNullException.ThrowIfNull(characterCache);

        _characterCache = characterCache ?? throw new ArgumentNullException(nameof(characterCache));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _characterCache.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}");
            }
            
            await Task.Delay(_interval, stoppingToken);
        }
    }
}