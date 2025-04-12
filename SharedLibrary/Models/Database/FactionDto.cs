using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models.Database;

public class FactionDto
{
    [Key]
    [MaxLength(20)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<DungeonFactionDto> DungeonLinks { get; set; } = new List<DungeonFactionDto>();
}