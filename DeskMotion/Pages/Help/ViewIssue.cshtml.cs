using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DeskMotion.Data;
using DeskMotion.Models;
using System.Collections.Generic;

namespace DeskMotion.Pages.Help;

public class ViewIssueModel(ApplicationDbContext context) : PageModel
{
    public IssueReport Report { get; private set; } = default!;

    public IActionResult OnGet(Guid id, string? attachmentName = null, bool download = false)
    {
        var report = context.IssueReports
            .Include(r => r.Attachments)
            .Include(r => r.Comments).ThenInclude(c => c.Attachments)
            .FirstOrDefault(r => r.Id == id);

        if (report == null)
        {
            return NotFound();
        }

        Report = report;

        if (download && attachmentName != null)
        {
            var attachment = Report.Attachments.FirstOrDefault(a => a.FileName == attachmentName)
                             ?? Report.Comments.SelectMany(c => c.Attachments).FirstOrDefault(a => a.FileName == attachmentName);

            if (attachment == null)
            {
                return NotFound();
            }

            return File(attachment.Content, attachment.MimeType, attachment.FileName);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddComment(Guid id, string content, List<IFormFile>? attachments)
    {
        var report = context.IssueReports.Include(r => r.Comments).FirstOrDefault(r => r.Id == id);
        if (report == null)
        {
            return NotFound();
        }

        var newComment = new IssueComment
        {
            Author = User.Identity?.Name ?? "Unknown",
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        if (attachments != null)
        {
            foreach (var file in attachments)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                newComment.Attachments.Add(new IssueAttachment
                {
                    FileName = file.FileName,
                    MimeType = file.ContentType,
                    Content = memoryStream.ToArray()
                });
            }
        }

        report.Comments.Add(newComment);
        report.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUpdateStatus(Guid id, string status)
    {
        var report = context.IssueReports.Include(r => r.Events).FirstOrDefault(r => r.Id == id);
        if (report == null)
        {
            return NotFound();
        }

        report.Status = status;
        report.UpdatedAt = DateTime.UtcNow;

        report.Events.Add(new IssueEvent
        {
            Description = $"Status updated to {status} by {User.Identity?.Name ?? "Unknown"}",
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }
}
