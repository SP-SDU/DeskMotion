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

namespace DeskMotion.Pages.Admin.Metadata;

public class EditModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public DeskMetadata DeskMetadata { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var deskMetadata = await context.DeskMetadata.FirstOrDefaultAsync(m => m.Id == id);
        if (deskMetadata == null)
        {
            return NotFound();
        }
        DeskMetadata = deskMetadata;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        context.Attach(DeskMetadata).State = EntityState.Modified;

        try
        {
            _ = await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!DeskMetadataExists(DeskMetadata.Id))
        {
            return NotFound();
        }

        return RedirectToPage("./Index");
    }

    private bool DeskMetadataExists(Guid id)
    {
        return context.DeskMetadata.Any(e => e.Id == id);
    }
}
