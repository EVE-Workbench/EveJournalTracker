using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models.Database;

public class EveSystemDto
{
    [Key]
    public int SystemId { get; set; }
    
    public int ConstellationId { get; set; }
    
    [MaxLength(255)]
    public string Name { get; set; } = null!;
    
    [MaxLength(32)]
    public string SecurityClass { get; set; } = null!;
    
    public double SecurityStatus { get; set; }
}