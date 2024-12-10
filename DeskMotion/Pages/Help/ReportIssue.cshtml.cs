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

    public void OnGet()
    {
    }
    public async Task<IActionResult> OnPostAsync(IFormFile? file)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            logger.LogWarning("Invalid UserId: {UserId}", userId);
            return Unauthorized();
        }

        IssueReport.UserId = userGuid;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (file != null && file.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            IssueReport.Attachment = memoryStream.ToArray();
            IssueReport.AttachmentFileName = file.FileName;
        }

        _ = context.IssueReports.Add(IssueReport);
        _ = await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
