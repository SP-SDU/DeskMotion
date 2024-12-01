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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages.Admin.Users;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public DeleteModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public User? IdentityUser { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        IdentityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (IdentityUser == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        IdentityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (IdentityUser != null)
        {
            var result = await _userManager.DeleteAsync(IdentityUser);

            if (result.Succeeded)
            {
                return RedirectToPage("./Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return Page();
    }
}
