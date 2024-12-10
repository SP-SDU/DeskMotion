using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Models;

[Owned]
public class IssueComment
{
    public string Author { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<IssueAttachment> Attachments { get; set; } = [];
}
