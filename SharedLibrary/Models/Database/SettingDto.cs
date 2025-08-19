using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models.Database;

public class SettingDto
{
    [Key]
    [MaxLength(50)]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;
    
    public DateTime? LastUpdated { get; set; }
}