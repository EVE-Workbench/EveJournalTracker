using SharedLibrary.Models.Database;

namespace SharedLibrary.Models;

public class EveSystem
{
    public EveSystemDto EveSystemDto { get; set; }
    
    public int Bounty { get; set; }
    
    public DateTime LastUpdated { get; set; }
}