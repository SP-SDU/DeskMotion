using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using DeskMotion.Data;
using DeskMotion.Models;

namespace DeskMotion.Pages.Help;

public class MyReportsModel(ApplicationDbContext context, UserManager<User> userManager) : PageModel
{
    public List<IssueReport> UserReports { get; private set; } = [];

    public void OnGet()
    {
        var userId = userManager.GetUserId(User);

        if (userId != null && Guid.TryParse(userId, out var userGuid))
        {
            UserReports = [.. context.IssueReports.Where(report => report.UserId == userGuid)];
        }
        else
        {
            UserReports = [];
        }
    }
}
