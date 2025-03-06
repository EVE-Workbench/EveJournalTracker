using SharedLibrary.Enums;

namespace SharedLibrary.Models;

public class Dungeon
{
    public Guid DungeonId { get; set; }
    public DungeonType Type { get; set; }
    public string Name { get; set; } = null!;
    public List<string>? Levels { get; set; }
    public string? Rating { get; set; }
    public Dictionary<int, string>? Factions { get; set; }
}