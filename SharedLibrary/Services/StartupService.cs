using Microsoft.Extensions.Logging;
using SharedLibrary.Data;
using SharedLibrary.Models.Database;

namespace SharedLibrary.Services;

public class StartupService
{
    private readonly AppDbContext _dbContext;
    private readonly EwbApiClientService _ewbApiClientService;
    private readonly ILogger<StartupService> _logger;

    public StartupService(AppDbContext dbContext, EwbApiClientService ewbApiClientService, ILogger<StartupService> logger)
    {
        _dbContext = dbContext;
        _ewbApiClientService = ewbApiClientService;
        _logger = logger;
    }
    
    public async void Initialize()
    {
        // check if we have any eveSystems in the database
        if (!_dbContext.EveSystems.Any())
        {
            // if not, import them from the ewb api
            var result = await ImportEveSystems();
            if (result)
            {
                _logger.LogInformation("Eve systems imported successfully.");
            }
            else
            {
                _logger.LogError("Failed to import eve systems.");
            }
        }
        else
        {
            _logger.LogInformation("Eve systems already exist in the database.");
        }
    }
    
    private async Task<bool> ImportEveSystems()
    {
        try
        {
            var ewbResponse = await _ewbApiClientService.GetEveSystems();
            // fetch eveSystems from ewb api

            if (ewbResponse == null) return true;
            
            var idCounter = 1;
            foreach (var eveSystem in ewbResponse)
            {
                eveSystem.SystemId = idCounter++;
            }
                
            _dbContext.EveSystems.AddRange(ewbResponse);
            _dbContext.SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }
}