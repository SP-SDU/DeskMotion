using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DeskMotion.Data;
using DeskMotion.Models;

namespace DeskMotion.Pages.Admin.Desks
{
    public class DeleteDeskModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeleteDeskModel> _logger;

        public DeleteDeskModel(ApplicationDbContext context, ILogger<DeleteDeskModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Desk? Desk { get; set; }
        public string? ErrorMessage { get; set; }


        public async Task<IActionResult> OnGetAsync(Guid? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            Desk = await _context.Desks
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Desk == null)
            {
                return NotFound();
            }

            if (Desk == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ErrorMessage = String.Format("Delete {ID} failed. Try again", id);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var desk = await _context.Desks.FindAsync(id);

            if (desk == null)
            {
                return NotFound();
            }

            try
            {
                _context.Desks.Remove(desk);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting desk {ID}", id);
                return RedirectToAction("./Delete", new { id, saveChangesError = true });
            }
        }
    }
}
