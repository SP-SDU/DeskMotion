using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeskMotion.Models;
using DeskMotion.Data;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages.Admin.ManageReports;

public class IndexModel(ApplicationDbContext context) : PageModel
{
    public IList<IssueReport> IssueReports { get; set; } = default!;

    public async Task OnGetAsync()
    {
        IssueReports = await context.IssueReports.ToListAsync();
    }

    public async Task<IActionResult> OnPostRespondAsync(Guid id)
    {
        var report = await context.IssueReports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        report.UpdatedAt = DateTime.UtcNow;
        _ = await context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCloseAsync(Guid id)
    {
        var report = await context.IssueReports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        report.Status = "Closed";
        report.UpdatedAt = DateTime.UtcNow;
        _ = await context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, string status)
    {
        var report = await context.IssueReports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        report.Status = status;
        report.UpdatedAt = DateTime.UtcNow;
        _ = await context.SaveChangesAsync();

        return RedirectToPage();
    }
}
