// Copyright 2024 PET Group16
//
// Licensed under the Apache License, Version 2.0 (the "License"):
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using DeskMotion.Data;
using DeskMotion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages.Admin.Desks;

public class DeleteModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public Desk Desk { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var desk = await context.Desks.FirstOrDefaultAsync(m => m.Id == id);

        if (desk == null)
        {
            return NotFound();
        }
        else
        {
            Desk = desk;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var desk = await context.Desks.FindAsync(id);
        if (desk != null)
        {
            Desk = desk;
            _ = context.Desks.Remove(Desk);
            _ = await context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
