using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models;

public class EveSystem
{
    [Key]
    public int SystemId { get; set; }
    
    public int ConstellationId { get; set; }
    
    public string Name { get; set; }
    
    public string SecurityClass { get; set; }
    
    public double SecurityStatus { get; set; }
}