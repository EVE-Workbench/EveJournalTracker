using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models.Database;

public class DungeonLevelDto
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid DungeonId { get; set; }

    [Required]
    public int Level { get; set; }

    [ForeignKey(nameof(DungeonId))]
    public virtual DungeonDto Dungeon { get; set; } = null!;
}