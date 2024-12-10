using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Models;

[Owned]
public class IssueAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = "application/octet-stream";
    public byte[] Content { get; set; } = [];
}
