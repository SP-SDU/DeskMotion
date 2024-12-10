using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeskMotion.Models;

namespace DeskMotion.Pages.Account.Manage;

public class RoleToolsModel(UserManager<User> userManager) : PageModel
{
    public bool IsAdminRole { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        IsAdminRole = await userManager.IsInRoleAsync(user, "Administrator");

        return Page();
    }
}
