using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeskMotion.Models;
using DeskMotion.Data;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages.Admin.ManageReports;

public class IndexModel(ApplicationDbContext context) : PageModel
{
    public List<IssueReport> IssueReports { get; private set; } = [];

    public void OnGet()
    {
        IssueReports = [.. context.IssueReports];
    }
}
