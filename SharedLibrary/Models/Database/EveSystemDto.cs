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
            return SecurityStatus switch
            {
                >= 1.0 => new SolidColorBrush(Color.FromRgb(71, 159, 241)),
                >= 0.9 => new SolidColorBrush(Color.FromRgb(90, 194, 245)),
                >= 0.8 => new SolidColorBrush(Color.FromRgb(116, 232, 252)),
                >= 0.7 => new SolidColorBrush(Color.FromRgb(139, 240, 204)),
                >= 0.6 => new SolidColorBrush(Color.FromRgb(156, 244, 123)),
                >= 0.5 => new SolidColorBrush(Color.FromRgb(250, 253, 174)),
                >= 0.4 => new SolidColorBrush(Color.FromRgb(237, 148, 12)),
                >= 0.3 => new SolidColorBrush(Color.FromRgb(230, 102, 26)),
                >= 0.2 => new SolidColorBrush(Color.FromRgb(216, 29, 37)),
                >= 0.1 => new SolidColorBrush(Color.FromRgb(158, 55, 55)),
                _ => new SolidColorBrush(Color.FromRgb(183, 81, 142))
            };
        }
    }
}