// Copyright 2024 PET Group16
//
// Licensed under the Apache License, Version 2.0 (the "License"):
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using DeskMotion.Data;
using DeskMotion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages.Help.Issues;

public class DetailsModel(ApplicationDbContext context) : PageModel
{
    public IssueReport Report { get; private set; } = default!;
    public List<object> Timeline { get; private set; } = [];
    public string Author { get; private set; } = string.Empty;

    public IActionResult OnGet(Guid id, string? attachmentName = null, bool download = false)
    {
        // Load issue report with comments and events
        var report = context.IssueReports
            .Include(r => r.Attachments)
            .Include(r => r.Comments).ThenInclude(c => c.Attachments)
            .Include(r => r.Events)
            .FirstOrDefault(r => r.Id == id);

        if (report == null)
        {
            return NotFound();
        }

        Report = report;

        var user = context.Users.FirstOrDefault(u => u.Id == Report.UserId);
        Author = user?.UserName ?? "Unknown";

        // Handle attachment downloads
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

        // Build timeline (combine comments and events)
        Timeline = Report.Comments.Cast<object>().ToList();
        Timeline.AddRange(Report.Events);
        Timeline = Timeline.OrderBy(e => e is IssueComment c ? c.CreatedAt : ((IssueEvent) e).Timestamp).ToList();

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

        _ = await context.SaveChangesAsync();

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
            Description = $"Status updated to {status}",
            Timestamp = DateTime.UtcNow
        });

        _ = await context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }
}
