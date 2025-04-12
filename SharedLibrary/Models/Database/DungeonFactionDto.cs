using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models.Database;

public class DungeonFactionDto
{
    [Required]
    public Guid DungeonId { get; set; }

    [Required]
    public int FactionId { get; set; }

    [ForeignKey(nameof(DungeonId))]
    public virtual DungeonDto Dungeon { get; set; } = null!;

    [ForeignKey(nameof(FactionId))]
    public virtual FactionDto Faction { get; set; } = null!;
}