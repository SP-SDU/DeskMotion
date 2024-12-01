using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DeskMotion.Data;
using DeskMotion.Models;

namespace DeskMotion.Pages.Admin.Desks;

    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Desk>? Desk { get;set; }

        public async Task OnGetAsync()
        {
            Desk = await _context.Desks.ToListAsync();
        }
    }
