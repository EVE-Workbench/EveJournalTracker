using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models.Database;

public class CharacterDto
{
    [Key]
    public required int CharacterId { get; set; }
    
    [MaxLength(255)]
    public required string Name { get; set; }
    
    public bool Online { get; set; }
    
    public int Bounty { get; set; }
    
    public DateTime LastUpdated { get; set; }
    
    public int? EveSystemId { get; set; }
    
    public bool Active { get; set; } = true;
}