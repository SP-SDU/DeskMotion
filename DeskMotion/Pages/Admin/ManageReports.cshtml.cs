using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeskMotion.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages.Admin
{
    public class ManageReportsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageReportsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<IssueReport> IssueReports { get; set; }

        public async Task OnGetAsync()
        {
            IssueReports = await _context.IssueReports.ToListAsync();
        }

        public async Task<IActionResult> OnPostRespondAsync(Guid id)
        {
            var report = await _context.IssueReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            // Logic to respond to the report

            report.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCloseAsync(Guid id)
        {
            var report = await _context.IssueReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            report.Status = "Closed";
            report.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, string status)
        {
            var report = await _context.IssueReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            report.Status = status;
            report.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
