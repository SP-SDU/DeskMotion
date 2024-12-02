using System.ComponentModel.DataAnnotations;

namespace DeskMotion.Models;

public class DeskAssignment
{
    [Key]
    public int Id { get; set; }
    
    public string DeskId { get; set; } = null!;
    public DeskInfo Desk { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    public bool IsTemporary => ExpiresAt.HasValue;
    public AssignmentStatus Status { get; set; }
}

public enum AssignmentStatus
{
    Active,
    Pending,    // For future admin approval
    Expired,
    Cancelled
}
