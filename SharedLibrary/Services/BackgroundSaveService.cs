using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedLibrary.Cache;

namespace SharedLibrary.Services;

public class BackgroundSaveService : BackgroundService
{
    private readonly CharacterCache _characterCache;
    private readonly ILogger<BackgroundSaveService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public BackgroundSaveService(CharacterCache characterCache, ILogger<BackgroundSaveService> logger)
    {
        ArgumentNullException.ThrowIfNull(characterCache);

        _characterCache = characterCache;
        _logger = logger;
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
                _logger.LogError(ex, "Error saving character changes");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}