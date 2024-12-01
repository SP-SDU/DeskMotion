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
using Microsoft.Extensions.DependencyInjection;

namespace DeskMotion.Pages.Admin.Desks;

    public class DetailsDeskModel(ApplicationDbContext _context) : PageModel
    {
        [BindProperty]
        public Desk? Desk { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Desk =  await _context.Desks.FirstOrDefaultAsync(d => d.Id == id);
            //Desk = await _context.Desks.Include(u => u.Reservations).ThenInclude(d => d.User).AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

        if (Desk == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
        if (!ModelState.IsValid)
        {

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                // Log or inspect the error messages
                Console.WriteLine(error.ErrorMessage);
            }
            return Page();
        }

        if (id == null)
            {
                return NotFound();
            }

            var deskToUpdate = await _context.Desks.FirstOrDefaultAsync(d => d.Id == id);

            if (deskToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Desk>(
                deskToUpdate,
                "desk",
                d => d.Location))
            {
                _ = await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            return Page();
        }
    }
