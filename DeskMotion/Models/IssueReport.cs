namespace DeskMotion.Models;

public class IssueReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid UserId { get; set; } = default!;
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public List<IssueComment> Comments { get; set; } = [];
    public List<IssueAttachment> Attachments { get; set; } = [];
    public List<IssueEvent> Events { get; set; } = [];
}
