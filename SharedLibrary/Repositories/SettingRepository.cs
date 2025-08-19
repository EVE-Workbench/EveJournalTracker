using Common.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedLibrary.Data;
using SharedLibrary.Models.Database;
using SharedLibrary.Repositories.Interfaces;

namespace SharedLibrary.Repositories;


public class SettingRepository : GenericRepository<SettingDto, string>, ISettingRepository
{
    private readonly AppDbContext _context;

    public SettingRepository(AppDbContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    {
        _context = context;
    }

    // Add convenient method for getting settings by key
    public async Task<SettingDto> GetByKeyAsync(string key)
    {
        return await _context.Settings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key) ?? new SettingDto();
    }

    // Add convenient upsert method
    public async Task<bool> UpsertAsync(string key, string value)
    {
        try
        {
            var existingSetting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (existingSetting != null)
            {
                existingSetting.Value = value;
                existingSetting.LastUpdated = DateTime.Now;
                _context.Settings.Update(existingSetting);
            }
            else
            {
                var newSetting = new SettingDto
                {
                    Key = key,
                    Value = value,
                    LastUpdated = DateTime.Now
                };
                await _context.Settings.AddAsync(newSetting);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // Log error but return false
            System.Diagnostics.Debug.WriteLine($"Error upserting setting '{key}': {ex.Message}");
            return false;
        }
    }
}