using Microsoft.Extensions.Logging;
using SharedLibrary.Data;
using SharedLibrary.Enums;
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
        
        if (!_dbContext.Dungeons.Any())
        {
            var result = await ImportDungeons();
            _logger.LogInformation(result
                ? "Dungeons imported successfully."
                : "Failed to import dungeons.");
        }
        else
        {
            _logger.LogInformation("Dungeons already exist in the database.");
        }
    }
    
    private async Task<bool> ImportEveSystems()
    {
        try
        {
            // fetch eveSystems from ewb api
            var ewbResponse = await _ewbApiClientService.GetEveSystems();
            if (ewbResponse == null) return true;
            
            var idCounter = 1;
            foreach (var eveSystem in ewbResponse)
            {
                eveSystem.SystemId = idCounter++;
            }
                
            _dbContext.EveSystems.AddRange(ewbResponse);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }
    
    private async Task<bool> ImportDungeons()
    {
        try
        {
            foreach (var type in Enum.GetValues<DungeonType>())
            {
                // if it is custom type, skip it, it is not in the api
                if (type == DungeonType.Custom) continue;
                
                // check if we have any dungeons of this type in the database, if so clean them up
                var existingDungeons = _dbContext.Dungeons.Where(d => d.Type == type).ToList();
                if (existingDungeons.Count != 0)
                {
                    _dbContext.Dungeons.RemoveRange(existingDungeons);
                    await _dbContext.SaveChangesAsync();
                }
                
                var apiDungeons = await _ewbApiClientService.GetDungeonsByType(type);
                if (apiDungeons == null) continue;

                foreach (var apiDungeon in apiDungeons)
                {
                    var dungeon = new DungeonDto
                    {
                        Id = apiDungeon.Id,
                        Name = apiDungeon.Name,
                        Type = type,
                        Rating = apiDungeon.Rating,
                        Levels = apiDungeon.Levels.Select(level => new DungeonLevelDto()
                        {
                            Level = level,
                        }).ToList(),
                        Factions = apiDungeon.Factions.Select(f => 
                        {
                            var faction = _dbContext.Factions.Find(int.Parse(f.Key)) ?? new FactionDto() { Id = int.Parse(f.Key), Name = f.Value };
                            return new DungeonFactionDto() { Faction = faction };
                        }).ToList()
                    };

                    _dbContext.Dungeons.Add(dungeon);
                }
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Dungeons imported successfully.");
            
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception occurred while importing dungeons.");
            return false;
        }
    }
}