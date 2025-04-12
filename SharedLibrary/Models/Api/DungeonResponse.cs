using SharedLibrary.Enums;

namespace SharedLibrary.Models.Api;

public class DungeonResponse
{
    public Guid Id { get; set; }
    public DungeonType Type { get; set; } 
    public string Name { get; set; } = string.Empty;
    public List<int> Levels { get; set; } = new();
    public string? Rating { get; set; }
    public Dictionary<string, string> Factions { get; set; } = new();
}