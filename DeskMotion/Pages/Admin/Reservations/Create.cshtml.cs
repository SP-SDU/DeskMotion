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

namespace DeskMotion.Pages.Admin.Reservations;

public class CreateModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public Reservation Reservation { get; set; } = default!;

    [BindProperty]
    public DateTime StartTimeLocal { get; set; } = DateTime.UtcNow;

    [BindProperty]
    public DateTime EndTimeLocal { get; set; } = DateTime.UtcNow;

    public SelectList Users { get; set; } = default!;
    public SelectList Desks { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        Users = new SelectList(
            await context.Users.Select(u => new { u.Id, u.UserName }).ToListAsync(),
            "Id",
            "UserName");

        Desks = new SelectList(
            await context.DeskMetadata.Select(d => new { d.Id, d.MacAddress }).ToListAsync(),
            "Id",
            "MacAddress");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Convert local time to UTC
        Reservation.StartTime = StartTimeLocal.ToUniversalTime();
        Reservation.EndTime = EndTimeLocal.ToUniversalTime();

        context.Reservations.Add(Reservation);
        await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
