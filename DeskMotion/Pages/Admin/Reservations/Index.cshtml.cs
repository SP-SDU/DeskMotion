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
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages.Admin.Reservations;

public class IndexModel(ApplicationDbContext context) : PageModel
{
    public IList<Reservation> Reservation { get; set; } = default!;
    public Dictionary<Guid, string> UserNames { get; set; } = [];
    public Dictionary<Guid, string> DeskMacAddresses { get; set; } = [];

    public async Task OnGetAsync()
    {
        Reservation = await context.Reservations.ToListAsync();

        var userIds = Reservation.Select(r => r.UserId).Distinct();
        var deskIds = Reservation.Select(r => r.DeskMetadataId).Distinct();

        UserNames = await context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName!);

        DeskMacAddresses = await context.DeskMetadata
            .Where(d => deskIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.MacAddress);
    }
}
