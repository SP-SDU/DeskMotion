using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeskMotion.Models;

public class DeskPreference
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid DeskId { get; set; }
    public DeskInfo Desk { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public double? DefaultHeight { get; set; }
    public List<ScheduledHeight> ScheduledHeights { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ScheduledHeight
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PreferenceId { get; set; }
    public DeskPreference Preference { get; set; } = null!;
    
    public TimeSpan StartTime { get; set; }
    public double Height { get; set; }
    public string? Note { get; set; }
}
