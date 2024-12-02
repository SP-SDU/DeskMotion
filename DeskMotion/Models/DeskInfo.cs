using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeskMotion.Models;

public class DeskInfo
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // This property maps to the old string DeskId for compatibility
    public Guid DeskId { get => Id; set => Id = value; }

    public string Name { get; set; } = null!;
    public string Location { get; set; } = "Main Office";
    public string Floor { get; set; } = "1";
    public string Department { get; set; } = "General";

    // Height management
    public double? CurrentHeight { get; set; }
    public double MinHeight { get; set; } = 60;
    public double MaxHeight { get; set; } = 120;
    public bool IsMoving { get; set; }

    // Status and configuration
    public string Status { get; set; } = null!;
    public string? Configuration { get; set; }
    public string? QRCodeData { get; set; }
    public DateTime LastUpdated { get; set; }

    // Assignment properties
    public bool IsAssigned { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public DateTime? AssignmentStart { get; set; }
    public DateTime? AssignmentEnd { get; set; }
    public bool IsTemporaryAssignment { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public List<HeightAdjustment> HeightAdjustments { get; set; } = new();
    public List<Reservation> Reservations { get; set; } = new();
    public List<DeskPreference> Preferences { get; set; } = new();
}

public class HeightAdjustment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public Guid DeskId { get; set; }
    public DateTime Timestamp { get; set; }
    public double OldHeight { get; set; }
    public double NewHeight { get; set; }
    public Guid? UserId { get; set; }
}
