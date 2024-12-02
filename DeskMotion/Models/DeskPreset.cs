using System.ComponentModel.DataAnnotations;

namespace DeskMotion.Models;

public class DeskPreset
{
    [Key]
    public int Id { get; set; }
    public string DeskId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public double Height { get; set; }
}
