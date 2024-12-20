using DeskMotion.Data;
using DeskMotion.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages.Reservations;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public CreateModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public Reservation Reservation { get; set; } = default!;

    [BindProperty]
    public DateTime StartTimeLocal { get; set; } = DateTime.Now.AddHours(1);

    [BindProperty]
    public DateTime EndTimeLocal { get; set; } = DateTime.Now.AddHours(2);

    public SelectList AvailableDesks { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Get all desks
        var desks = await _context.DeskMetadata
            .Select(d => new { d.Id, Name = $"Desk {d.Id}" })
            .ToListAsync();

        AvailableDesks = new SelectList(desks, "Id", "Name");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        // Set the user ID and convert times to UTC
        Reservation.UserId = user.Id;  
        Reservation.StartTime = StartTimeLocal.ToUniversalTime();
        Reservation.EndTime = EndTimeLocal.ToUniversalTime();

        // Check for overlapping reservations
        var hasOverlap = await _context.Reservations
            .AnyAsync(r => r.DeskMetadataId == Reservation.DeskMetadataId &&
                          r.EndTime > Reservation.StartTime &&
                          r.StartTime < Reservation.EndTime);

        if (hasOverlap)
        {
            ModelState.AddModelError(string.Empty, "This desk is already reserved for the selected time period.");
            await OnGetAsync();
            return Page();
        }

        _context.Reservations.Add(Reservation);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Dashboard");
    }
}
