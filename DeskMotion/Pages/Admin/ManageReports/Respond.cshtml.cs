using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeskMotion.Models;
using DeskMotion.Data;


namespace DeskMotion.Pages.Admin.ManageReports;

public class RespondModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public IssueReport Report { get; set; } = default!;

    public IActionResult OnGet(Guid id)
    {
        Report = context.IssueReports.Find(id)!;
        if (Report == null)
        {
            return NotFound();
        }
        return Page();
    }

    public IActionResult OnPost(Guid id, string response, string status)
    {
        var report = context.IssueReports.Find(id);
        if (report == null)
        {
            return NotFound();
        }

        report.Status = status;
        report.UpdatedAt = DateTime.UtcNow;

        _ = context.SaveChanges();

        return RedirectToPage("./Index");
    }
}
