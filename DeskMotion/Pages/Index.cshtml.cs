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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages;

public class IndexModel(ApplicationDbContext dbContext, UserManager<User> userManager) : PageModel
{
    public List<DeskMetadata> UserDesks { get; set; } = [];

    public async Task OnGetAsync()
    {
        // Get the current logged-in user
        var user = await userManager.GetUserAsync(User);

        if (user != null)
        {
            // Fetch desks belonging to the user
            UserDesks = await dbContext.DeskMetadata
                .Where(d => d.OwnerId == user.Id)
                .ToListAsync();
        }
    }
}
