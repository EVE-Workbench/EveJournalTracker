using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary.Models.Database;

namespace SharedLibrary.Repositories.Interfaces;

public interface IDungeonRepository : IGenericRepository<DungeonDto, Guid>
{

}
