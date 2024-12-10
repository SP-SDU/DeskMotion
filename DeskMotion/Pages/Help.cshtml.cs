using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeskMotion.Models;
using System.ComponentModel.DataAnnotations;

namespace DeskMotion.Pages
{
    public class HelpModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public HelpModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public IssueReport IssueReport { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.IssueReports.Add(IssueReport);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
