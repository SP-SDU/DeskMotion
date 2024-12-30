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
using System.ComponentModel.DataAnnotations;

namespace DeskMotion.Pages;

public class SetupModel(ApplicationDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager) : PageModel
{
    [BindProperty]
    [Required]
    [Display(Name = "Organization name")]
    public string OrganizationName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [EmailAddress]
    [Display(Name = "Admin email")]
    public string AdminEmail { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Admin password")]
    public string AdminPassword { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        if (context.InitialData.Any() || context.Users.Any())
        {
            return RedirectToPage("/Account/Login");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Create roles
        var roles = new[] { "Administrator", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                _ = await roleManager.CreateAsync(new Role { Name = role });
            }
        }

        // Create admin user
        var adminUser = new User
        {
            UserName = AdminEmail,
            Email = AdminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, AdminPassword);
        if (result.Succeeded)
        {
            _ = await userManager.AddToRoleAsync(adminUser, "Administrator");
        }

        // Save organization name
        var initialData = new InitialData
        {
            OrganizationName = OrganizationName
        };
        _ = context.InitialData.Add(initialData);
        _ = await context.SaveChangesAsync();

        return RedirectToPage("/Account/Login");
    }
}
