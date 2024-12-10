using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeskMotion.Data;
using DeskMotion.Models;

namespace DeskMotion.Pages.Help;

public class ViewIssueModel(ApplicationDbContext context) : PageModel
{
    public IssueReport Report { get; private set; } = default!;
    public string AdminResponse { get; private set; } = string.Empty;

    public IActionResult OnGet(Guid id, bool download = false)
    {
        var report = context.IssueReports.Find(id);
        if (report == null)
        {
            return NotFound();
        }
        Report = report;

        if (download && Report.Attachment != null)
        {
            var fileName = Report.AttachmentFileName ?? "Attachment";
            return File(Report.Attachment, "application/octet-stream", fileName);
        }

        AdminResponse = "Thank you for reporting this issue. We are currently working on a resolution.";

        return Page();
    }
}
