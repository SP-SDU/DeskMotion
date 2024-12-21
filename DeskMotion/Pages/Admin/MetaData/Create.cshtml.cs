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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages.Admin.Metadata;

public class CreateModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public DeskMetadata DeskMetadata { get; set; } = default!;

    public List<SelectListItem> AvailableDesks { get; set; } = [];
    public List<SelectListItem> AvailableUsers { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAvailableDesksAsync();
        await LoadAvailableUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAvailableDesksAsync();
            await LoadAvailableUsersAsync();
            return Page();
        }

        // Check if metadata already exists
        var existingMetadata = await context.DeskMetadata
            .FirstOrDefaultAsync(dm => dm.MacAddress == DeskMetadata.MacAddress);

        if (existingMetadata != null)
        {
            // Update the existing metadata
            existingMetadata.Location = DeskMetadata.Location;
            existingMetadata.QRCodeData = DeskMetadata.QRCodeData;
            existingMetadata.OwnerId = DeskMetadata.OwnerId;
        }
        else
        {
            // Add new metadata
            _ = context.DeskMetadata.Add(DeskMetadata);
        }

        _ = await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    private async Task LoadAvailableDesksAsync()
    {
        AvailableDesks = await context.Desks
            .Select(d => new SelectListItem { Value = d.MacAddress, Text = d.MacAddress })
            .Distinct()
            .ToListAsync();
    }

    private async Task LoadAvailableUsersAsync()
    {
        AvailableUsers = await context.Users
            .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Email })
            .ToListAsync();
    }
}
