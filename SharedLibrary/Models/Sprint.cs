using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models;

public class Sprint
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
} 