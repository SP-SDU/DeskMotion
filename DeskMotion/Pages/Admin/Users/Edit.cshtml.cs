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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages.Admin.Users;

public class EditModel(ApplicationDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager) : PageModel
{
    [BindProperty]
    public User? IdentityUser { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    [BindProperty]
    public string SelectedRole { get; set; } = string.Empty;

    public List<SelectListItem> Roles { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        IdentityUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (IdentityUser == null)
        {
            return NotFound();
        }

        Roles = [.. roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name })];

        var userRoles = await userManager.GetRolesAsync(IdentityUser);
        SelectedRole = userRoles.FirstOrDefault() ?? "User";

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid? id)
    {
        if (!ModelState.IsValid)
        {
            Roles = roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToList();
            return Page();
        }

        if (id == null)
        {
            return NotFound();
        }

        var userToUpdate = await userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (userToUpdate == null)
        {
            return NotFound();
        }

        if (await TryUpdateModelAsync<User>(
            userToUpdate,
            "IdentityUser",
            u => u.UserName, u => u.Email))
        {
            if (!string.IsNullOrEmpty(Password))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(userToUpdate);
                var result = await userManager.ResetPasswordAsync(userToUpdate, token, Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            // Update user's role
            var currentRoles = await userManager.GetRolesAsync(userToUpdate);
            _ = await userManager.RemoveFromRolesAsync(userToUpdate, currentRoles);
            _ = await userManager.AddToRoleAsync(userToUpdate, SelectedRole);

            _ = await context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        Roles = [.. roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name })];

        return Page();
    }
}
