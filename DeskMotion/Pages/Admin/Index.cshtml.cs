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
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages.Admin;

public class IndexModel(ApplicationDbContext context) : PageModel
{
    public int TotalDesks { get; set; }
    public int TotalMetadata { get; set; }
    public int TotalUsers { get; set; }
    public double AverageDeskUsageTimeMinutes { get; set; }

    public List<string> DeskUsageByLocationLabels { get; private set; } = [];
    public List<int> DeskUsageByLocationData { get; private set; } = [];
    public List<string> UserActivityLabels { get; private set; } = [];
    public List<int> UserActivityData { get; private set; } = [];
    public List<string> HealthInsightsLabels { get; private set; } = [];
    public List<int> HealthInsightsData { get; private set; } = [];
    public List<string> DeskHeightOverTimeLabels { get; private set; } = [];
    public List<double> DeskHeightOverTimeData { get; private set; } = [];

    public async Task OnGetAsync()
    {
        TotalDesks = await context.Desks.CountAsync();
        TotalMetadata = await context.DeskMetadata.CountAsync();
        TotalUsers = await context.Users.CountAsync();
        AverageDeskUsageTimeMinutes = await context.Desks
            .Where(d => d.Usage != null)
            .AverageAsync(d => d.Usage.ActivationsCounter * 10);

        var deskUsageByLocation = await context.DeskMetadata
            .GroupBy(dm => dm.Location)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        DeskUsageByLocationLabels = deskUsageByLocation.Keys.ToList();
        DeskUsageByLocationData = deskUsageByLocation.Values.ToList();

        var userActivity = await context.Users
            .GroupBy(u => u.CreatedAt.Date)
            .ToDictionaryAsync(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count());

        UserActivityLabels = userActivity.Keys.ToList();
        UserActivityData = userActivity.Values.ToList();

        var healthInsights = await context.Desks
            .Where(d => d.State != null)
            .GroupBy(d => d.State.IsPositionLost ? "Unhealthy" : "Healthy")
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        HealthInsightsLabels = healthInsights.Keys.ToList();
        HealthInsightsData = healthInsights.Values.ToList();

        var deskHeightOverTime = await context.Desks
            .GroupBy(d => d.RecordedAt.Date)
            .ToDictionaryAsync(g => g.Key.ToString("yyyy-MM-dd"), g => g.Average(d => d.State.Position_mm / 10.0));

        DeskHeightOverTimeLabels = deskHeightOverTime.Keys.ToList();
        DeskHeightOverTimeData = deskHeightOverTime.Values.ToList();
    }
}
