using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using DeskMotion.Models;
using DeskMotion.Data;
using System.Collections.Generic;

namespace DeskMotion.Pages.Help;

public class ReportIssueModel(ApplicationDbContext context, UserManager<User> userManager, ILogger<ReportIssueModel> logger) : PageModel
{
    [BindProperty]
    public IssueReport IssueReport { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(List<IFormFile>? attachments)
    {
        // Validate User
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            logger.LogWarning("Invalid UserId: {UserId}", userId);
            return Unauthorized();
        }

        IssueReport.UserId = userGuid;

        // Validate Model
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Handle Attachments
        if (attachments != null && attachments.Any())
        {
            foreach (var file in attachments)
            {
                if (file.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);

                    IssueReport.Attachments.Add(new IssueAttachment
                    {
                        FileName = file.FileName,
                        MimeType = file.ContentType,
                        Content = memoryStream.ToArray()
                    });
                }
            }
        }

        // Save Issue Report
        _ = context.IssueReports.Add(IssueReport);
        _ = await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
