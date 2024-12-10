using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Models;

[Owned]
public class IssueEvent
{
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
