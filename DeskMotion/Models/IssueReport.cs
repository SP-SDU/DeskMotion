using System;
using System.ComponentModel.DataAnnotations;

namespace DeskMotion.Models;

public class IssueReport
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public string Status { get; set; } = "Open";

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public byte[]? Attachment { get; set; }
}
