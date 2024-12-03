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

namespace DeskMotion.Pages.Admin.Reservations;

public class DetailsModel(ApplicationDbContext context) : PageModel
{
    public Reservation Reservation { get; set; } = default!;
    public string UserName { get; set; } = string.Empty;
    public string DeskMacAddress { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Fetch the reservation
        var reservation = await context.Reservations.FirstOrDefaultAsync(m => m.Id == id);
        if (reservation == null)
        {
            return NotFound();
        }
        Reservation = reservation;

        // Fetch the username
        UserName = await context.Users
            .Where(u => u.Id == Reservation.UserId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync() ?? "Unknown User";

        // Fetch the desk MAC address
        DeskMacAddress = await context.DeskMetadata
            .Where(d => d.Id == Reservation.DeskMetadataId)
            .Select(d => d.MacAddress)
            .FirstOrDefaultAsync() ?? "Unknown Desk";

        return Page();
    }
}
