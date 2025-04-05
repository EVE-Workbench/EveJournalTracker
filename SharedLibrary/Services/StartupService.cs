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
        try
        {
            // check if we have any eveSystems in the database
            if (_dbContext.EveSystems.Any()) return;
            
            var ewbResponse = await _ewbApiClientService.GetEveSystems();
            // fetch eveSystems from ewb api

            if (ewbResponse != null)
            {
                int idCounter = 1;
                foreach (var eveSystem in ewbResponse)
                {
                    eveSystem.SystemId = idCounter++;
                    // set the system id to the counter
                }
                
                _dbContext.EveSystems.AddRange(ewbResponse);
                _dbContext.SaveChanges();
            }
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }
}