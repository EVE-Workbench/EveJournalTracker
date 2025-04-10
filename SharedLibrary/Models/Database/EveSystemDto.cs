using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;

namespace SharedLibrary.Models.Database;

public class EveSystemDto
{
    [Key]
    public int SystemId { get; set; }
    
    public int ConstellationId { get; set; }
    
    [MaxLength(255)]
    public string Name { get; set; } = null!;
    
    [MaxLength(32)]
    public string? SecurityClass { get; set; } = null!;
    
    public double? SecurityStatus { get; set; }
    
    [NotMapped]
    public SolidColorBrush SecurityColor
    {
        get
        {
            if (SecurityStatus <= 0.1)
                return Brushes.Red;
            else if (SecurityStatus <= 0.5)
                return Brushes.Orange;
            else
                return Brushes.Green;
        }
    }
}