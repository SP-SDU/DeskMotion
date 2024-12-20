using DeskMotion.Data;
using DeskMotion.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace DeskMotion.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public DashboardModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Properties for user dashboard data
    public int ActiveReservations { get; set; }
    public double TodayDeskUsageHours { get; set; }
    public double AverageStandingTimePercentage { get; set; }
    public double PreferredDeskHeight { get; set; }
    // ... other properties for charts etc.

    public string DeskUsageByTimeChartConfig { get; private set; } = string.Empty;
    public string HealthInsightsChartConfig { get; private set; } = string.Empty;
    public string DeskHeightOverTimeChartConfig { get; private set; } = string.Empty;
    public string StandingSittingRatioChartConfig { get; private set; } = string.Empty;


    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return; // Handle user not found
        }

        // Ensure user has "User" role if not an admin
        if (!await _userManager.IsInRoleAsync(user, "Administrator") && !await _userManager.IsInRoleAsync(user, "User"))
        {
            await _userManager.AddToRoleAsync(user, "User");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return; // Handle userId not found
        }

        // Fetch data for user-specific dashboard

        var now = DateTime.UtcNow;
        ActiveReservations = await _context.Reservations
            .CountAsync(r => r.UserId == Guid.Parse(userId) && r.StartTime <= now && r.EndTime >= now);

        var userDesks = await _context.Reservations
            .Where(r => r.UserId == Guid.Parse(userId))
            .Select(r => r.DeskMetadataId)
            .ToListAsync();

        if (userDesks.Count > 0)
        {
            var today = DateTime.UtcNow.Date;
            var deskUsageToday = await _context.Desks
                .Include(d => d.State)
                .Include(d => d.Usage)
                .Where(d => userDesks.Contains(d.Id) && d.RecordedAt.Date == today)
                .ToListAsync();

            TodayDeskUsageHours = deskUsageToday.Sum(d => d.Usage?.ActivationsCounter * 10.0 / 60.0 ?? 0);

            var totalPositions = deskUsageToday.Count(d => d.State != null);
            var standingPositions = deskUsageToday.Count(d => d.State?.Position_mm > 800);

            AverageStandingTimePercentage = totalPositions > 0 ? (standingPositions * 100.0 / totalPositions) : 0;

            PreferredDeskHeight = await _context.Desks
                .Where(d => userDesks.Contains(d.Id) && d.State != null)
                .Select(d => (double?)d.State.Position_mm / 10.0)
                .AverageAsync() ?? 0;


            var usageByHour = await _context.Desks
                .Include(d => d.State)
                .Where(d => userDesks.Contains(d.Id) && d.RecordedAt >= DateTime.UtcNow.AddDays(-7))
                .GroupBy(d => d.RecordedAt.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderBy(x => x.Hour)
                .ToDictionaryAsync(x => x.Hour.ToString("D2"), x => x.Count);

            DeskUsageByTimeChartConfig = JsonSerializer.Serialize(GetChartConfig(usageByHour, "bar", "Desk Usage by Hour"));

            var healthData = new Dictionary<string, int>
            {
                { "Standing Time", (int)AverageStandingTimePercentage },
                { "Sitting Time", 100 - (int)AverageStandingTimePercentage }
            };

            HealthInsightsChartConfig = JsonSerializer.Serialize(GetChartConfig(healthData, "doughnut"));

            var heightOverTime = await _context.Desks
                .Include(d => d.State)
                .Where(d => userDesks.Contains(d.Id) && d.State != null && d.RecordedAt >= DateTime.UtcNow.AddDays(-7))
                .GroupBy(d => d.RecordedAt.Date)
                .Select(g => new { Date = g.Key, Height = g.Average(d => d.State.Position_mm / 10.0) })
                .OrderBy(x => x.Date)
                .ToDictionaryAsync(x => x.Date.ToString("MM/dd"), x => x.Height);

            DeskHeightOverTimeChartConfig = JsonSerializer.Serialize(GetChartConfig(heightOverTime, "line", "Average Desk Height (cm)"));

            var standingSittingRatio = await _context.Desks
                .Include(d => d.State)
                .Where(d => userDesks.Contains(d.Id) && d.State != null && d.RecordedAt >= DateTime.UtcNow.AddDays(-7))
                .GroupBy(d => d.RecordedAt.Date)
                .Select(g => new { Date = g.Key, StandingRatio = g.Count(d => d.State.Position_mm > 800) * 100.0 / g.Count() })
                .OrderBy(x => x.Date)
                .ToDictionaryAsync(x => x.Date.ToString("MM/dd"), x => x.StandingRatio);

            StandingSittingRatioChartConfig = JsonSerializer.Serialize(GetChartConfig(standingSittingRatio, "line", "Standing Time (%)", true));
        }
    }

    private object GetChartConfig(Dictionary<string, int> data, string type, string label = null, bool percentage = false)
    {
        return new
        {
            type = type,
            data = new
            {
                labels = data.Keys,
                datasets = new[]
                {
                    new
                    {
                        label = label,
                        data = data.Values,
                        backgroundColor = GetBackgroundColor(type),
                        borderColor = GetBorderColor(type),
                        borderWidth = 1,
                        fill = type == "line"
                    }
                }
            },
            options = percentage ? new { scales = new { y = new { beginAtZero = true, max = 100 } } } : null
        };
    }

        private object GetChartConfig(Dictionary<string, double> data, string type, string label = null, bool percentage = false)
    {
        return new
        {
            type = type,
            data = new
            {
                labels = data.Keys,
                datasets = new[]
                {
                    new
                    {
                        label = label,
                        data = data.Values,
                        backgroundColor = GetBackgroundColor(type),
                        borderColor = GetBorderColor(type),
                        borderWidth = 1,
                        fill = type == "line"
                    }
                }
            },
            options = percentage ? new { scales = new { y = new { beginAtZero = true, max = 100 } } } : null
        };
    }


    private string[] GetBackgroundColor(string type)
    {
        return type == "bar" ? new[] { "rgba(75, 192, 192, 0.2)" } : new[] { "#4BC0C0", "#FF6384" };
    }

    private string[] GetBorderColor(string type)
    {
        return type == "bar" ? new[] { "rgba(75, 192, 192, 1)" } : new[] { "#4BC0C0", "#FF6384" };
    }
}
