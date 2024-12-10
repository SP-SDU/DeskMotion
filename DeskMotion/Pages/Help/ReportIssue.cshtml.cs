using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using DeskMotion.Models;
using DeskMotion.Data;

namespace DeskMotion.Pages.Help;

public class ReportIssueModel(ApplicationDbContext context, UserManager<User> userManager, ILogger<ReportIssueModel> logger) : PageModel
{
    [BindProperty]
    public IssueReport IssueReport { get; set; } = new();

    public IActionResult OnGet()
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            logger.LogWarning("Invalid UserId: {UserId}", userId);
            return Unauthorized();
        }

        IssueReport.UserId = userGuid;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _ = context.IssueReports.Add(IssueReport);
        _ = await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
