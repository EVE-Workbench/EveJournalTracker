using System.ComponentModel.DataAnnotations;
using SharedLibrary.Enums;

namespace SharedLibrary.Models.Database;

public class DungeonDto
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DungeonType Type { get; set; }

    [MaxLength(20)]
    public string? Rating { get; set; }

    public virtual ICollection<DungeonLevelDto> Levels { get; set; } = new List<DungeonLevelDto>();
    public virtual ICollection<DungeonFactionDto> Factions { get; set; } = new List<DungeonFactionDto>();
}