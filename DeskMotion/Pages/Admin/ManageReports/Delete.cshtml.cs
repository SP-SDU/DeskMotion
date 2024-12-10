using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeskMotion.Models;
using DeskMotion.Data;

namespace DeskMotion.Pages.Admin.ManageReports;

public class DeleteModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public IssueReport Report { get; set; } = default!;

    public IActionResult OnGet(Guid id)
    {
        var report = context.IssueReports.Find(id);
        if (report == null)
        {
            return NotFound();
        }
        Report = report;

        return Page();
    }

    public IActionResult OnPost(Guid id)
    {
        var report = context.IssueReports.Find(id);
        if (report == null)
        {
            return NotFound();
        }

        _ = context.IssueReports.Remove(report);
        _ = context.SaveChanges();

        return RedirectToPage("./Index");
    }
}
