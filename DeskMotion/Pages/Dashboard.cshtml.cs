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
                .Where(d => userDesks.Contains(d.Id) && d.RecordedAt.Date == today)
                .ToListAsync();

            TodayDeskUsageHours = deskUsageToday.Sum(d => d.Usage?.ActivationsCounter * 10.0 / 60.0 ?? 0);

            var totalPositions = deskUsageToday.Count();
            var standingPositions = deskUsageToday.Count(d => d.State.Position_mm > 800);

            AverageStandingTimePercentage = totalPositions > 0 ? (standingPositions * 100.0 / totalPositions) : 0;

            var deskHeights = await _context.Desks
                .Where(d => userDesks.Contains(d.Id))
                .Select(d => new { Height = d.State.Position_mm })
                .ToListAsync();

            PreferredDeskHeight = deskHeights.Any() 
                ? deskHeights.Average(d => d.Height / 10.0)
                : 0;

            var usageData = await _context.Desks
                .Where(d => userDesks.Contains(d.Id) && d.RecordedAt >= DateTime.UtcNow.AddDays(-7))
                .Select(d => new { d.RecordedAt.Hour })
                .ToListAsync();

            var usageByHour = usageData
                .GroupBy(d => d.Hour)
                .ToDictionary(
                    g => g.Key.ToString("D2"),
                    g => g.Count()
                );

            DeskUsageByTimeChartConfig = JsonSerializer.Serialize(GetChartConfig(usageByHour, "bar", "Desk Usage by Hour"));

            var healthData = new Dictionary<string, int>
            {
                { "Standing Time", (int)AverageStandingTimePercentage },
                { "Sitting Time", 100 - (int)AverageStandingTimePercentage }
            };

            HealthInsightsChartConfig = JsonSerializer.Serialize(GetChartConfig(healthData, "doughnut"));

            var heightData = await _context.Desks
                .Where(d => userDesks.Contains(d.Id) && d.RecordedAt >= DateTime.UtcNow.AddDays(-7))
                .Select(d => new { 
                    Date = d.RecordedAt.Date,
                    Height = d.State.Position_mm / 10.0
                })
                .ToListAsync();

            var heightOverTime = heightData
                .GroupBy(d => d.Date)
                .ToDictionary(
                    g => g.Key.ToString("MM/dd"),
                    g => g.Average(d => d.Height)
                );

            DeskHeightOverTimeChartConfig = JsonSerializer.Serialize(GetChartConfig(heightOverTime, "line", "Average Desk Height (cm)"));

            var standingData = await _context.Desks
                .Where(d => userDesks.Contains(d.Id) && d.RecordedAt >= DateTime.UtcNow.AddDays(-7))
                .Select(d => new { 
                    Date = d.RecordedAt.Date,
                    IsStanding = d.State.Position_mm > 800
                })
                .ToListAsync();

            var standingSittingRatio = standingData
                .GroupBy(d => d.Date)
                .ToDictionary(
                    g => g.Key.ToString("MM/dd"),
                    g => g.Count(d => d.IsStanding) * 100.0 / g.Count()
                );

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
