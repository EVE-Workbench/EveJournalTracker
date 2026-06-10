using System.ComponentModel.DataAnnotations;
using SharedLibrary.Enums;
using SharedLibrary.Models.Database;

namespace SharedLibrary.Models;

public class LogEvent
{
    public int Id { get; set; }
    public LogEventType Type { get; set; }
    public int? BountyValue { get; set; }
    public string? Value { get; set; }
    public string? DamageType { get; set; }
    public string? DamageQuality { get; set; }
    public string Raw { get; set; } = null!;

    // True when the line was already in the gamelog at client start (initial backfill)
    // rather than appended live. The DPS meter ignores these so old combat is not
    // replayed through the real-time graph.
    public bool IsHistorical { get; set; }
    
    public int CharacterId { get; set; }
    public virtual Character Character { get; set; }
    
    public int? SprintId { get; set; }
    public virtual Sprint? Sprint { get; set; }
    public EveSystemDto? EveSystem { get; set; }
    public DateTime Timestamp { get; set; }
}