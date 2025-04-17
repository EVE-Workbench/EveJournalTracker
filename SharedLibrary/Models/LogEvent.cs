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
    
    public int CharacterId { get; set; }
    public virtual Character Character { get; set; }
    
    public int? SprintId { get; set; }
    public virtual Sprint? Sprint { get; set; }
    public EveSystemDto? EveSystem { get; set; }
    public DateTime Timestamp { get; set; }
}