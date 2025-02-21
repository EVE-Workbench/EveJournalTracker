using SharedLibrary.Enums;

namespace SharedLibrary.Models;

public class LogEvent
{
    public LogEventType Type { get; set; }
    public int? BountyValue { get; set; }
    public string? Value { get; set; }
    public string Raw { get; set; } = null!;
    public Character Character { get; set; } = null!;
    public Sprint? Sprint { get; set; }
    public EveSystem? EveSystem { get; set; }
    public DateTime Timestamp { get; set; }
}