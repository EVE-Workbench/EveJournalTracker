using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public string SecurityColorHex
    {
        get
        {
            return SecurityStatus switch
            {
                >= 1.0 => "#479FF1",
                >= 0.9 => "#5AC2F5",
                >= 0.8 => "#74E8FC",
                >= 0.7 => "#8BF0CC",
                >= 0.6 => "#9CF47B",
                >= 0.5 => "#FAFDAE",
                >= 0.4 => "#ED940C",
                >= 0.3 => "#E6661A",
                >= 0.2 => "#D81D25",
                >= 0.1 => "#9E3737",
                _ => "#B7518E"
            };
        }
    }
}