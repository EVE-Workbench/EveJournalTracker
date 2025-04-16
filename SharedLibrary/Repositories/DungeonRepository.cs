using Common.Repositories;
using Microsoft.Extensions.Logging;
using SharedLibrary.Data;
using SharedLibrary.Models.Database;
using SharedLibrary.Repositories.Interfaces;

namespace SharedLibrary.Repositories;


public class DungeonRepository : GenericRepository<DungeonDto, Guid>, IDungeonRepository
{
    public DungeonRepository(AppDbContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory )
    {
    }
}