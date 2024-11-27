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
    public class DetailsDeskModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsDeskModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Desk? Desk { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Desk = await _context.Desks.FirstOrDefaultAsync(m => m.Id == id);
            //Desk = await _context.Desks.Include(u => u.Reservations).ThenInclude(d => d.User).AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

            if (Desk == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
