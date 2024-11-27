using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DeskMotion.Data;
using DeskMotion.Models;

namespace DeskMotion.Pages.Admin.Desks
{
    public class EditDeskModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditDeskModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Desk? Desk { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Desk =  await _context.Desks.FindAsync(id);

            if (Desk == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            var DeskToUpdate = await _context.Desks.FindAsync(id);

            if (DeskToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Desk>(
                DeskToUpdate,
                "Desk",
                d => d.Location,d => d.QRCodeData, d => d.State,d => d.State.Position_mm, d => d.Config.Name, d => d.Config.Manufacturer, d => d.Usage.ActivationsCounter, d => d.Usage.SitStandCounter))
            {
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            return Page();
        }

    }
}
