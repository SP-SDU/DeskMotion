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

namespace DeskMotion.Pages.Admin.Users;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public CreateModel(ApplicationDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public IActionResult OnGet()
    {
        IdentityUser = new User
        {
            UserName = "",
            Email = ""
        };

        // Populate roles for dropdown
        Roles = [.. _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name })];

        // Default role
        SelectedRole = "User";

        return Page();
    }

    [BindProperty]
    public User? IdentityUser { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    [BindProperty]
    public string SelectedRole { get; set; } = "User";

    public List<SelectListItem> Roles { get; set; } = [];

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Roles = [.. _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name })];
            return Page();
        }

        var user = new User
        {
            UserName = IdentityUser!.Email,
            Email = IdentityUser.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, Password!);

        if (result.Succeeded)
        {
            // Assign selected role
            if (!string.IsNullOrEmpty(SelectedRole))
            {
                _ = await _userManager.AddToRoleAsync(user, SelectedRole);
            }

            return RedirectToPage("./Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        Roles = [.. _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name })];

        return Page();
    }
}
