// Copyright 2024 Web.Tech. Group17
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

using System.Text.Json;
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

    public string DeskUsageByLocationChartConfig { get; private set; } = string.Empty;
    public string UserActivityChartConfig { get; private set; } = string.Empty;
    public string HealthInsightsChartConfig { get; private set; } = string.Empty;
    public string DeskHeightOverTimeChartConfig { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        TotalDesks = await context.Desks.CountAsync();
        TotalMetadata = await context.DeskMetadata.CountAsync();
        TotalUsers = await context.Users.CountAsync();

        AverageDeskUsageTimeMinutes = await context.Desks
            .Where(d => d.Usage != null)
            .AverageAsync(d => d.Usage.ActivationsCounter * 10); // Example: 10 minutes per activation

        var deskUsageByLocation = await context.DeskMetadata
            .GroupBy(dm => dm.Location)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var userActivity = await context.Users
            .GroupBy(u => u.CreatedAt.Date)
            .ToDictionaryAsync(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count());

        var healthInsights = await context.Desks
            .Where(d => d.State != null)
            .GroupBy(d => d.State.IsPositionLost ? "Unhealthy" : "Healthy")
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var deskHeightOverTime = await context.Desks
            .GroupBy(d => d.RecordedAt.Date)
            .ToDictionaryAsync(g => g.Key.ToString("yyyy-MM-dd"), g => g.Average(d => d.State.Position_mm / 10.0)); // Convert mm to cm

        DeskUsageByLocationChartConfig = JsonSerializer.Serialize(new
        {
            type = "pie",
            data = new
            {
                labels = deskUsageByLocation.Keys,
                datasets = new[]
                {
                    new
                    {
                        data = deskUsageByLocation.Values,
                        backgroundColor = new[] { "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0" }
                    }
                }
            }
        });

        UserActivityChartConfig = JsonSerializer.Serialize(new
        {
            type = "line",
            data = new
            {
                labels = userActivity.Keys,
                datasets = new[]
                {
                    new
                    {
                        label = "User Registrations",
                        data = userActivity.Values,
                        backgroundColor = "rgba(75, 192, 192, 0.2)",
                        borderColor = "rgba(75, 192, 192, 1)",
                        borderWidth = 1
                    }
                }
            }
        });

        HealthInsightsChartConfig = JsonSerializer.Serialize(new
        {
            type = "bar",
            data = new
            {
                labels = healthInsights.Keys,
                datasets = new[]
                {
                    new
                    {
                        label = "Hours in Healthy Range",
                        data = healthInsights.Values,
                        backgroundColor = "rgba(54, 162, 235, 0.2)",
                        borderColor = "rgba(54, 162, 235, 1)",
                        borderWidth = 1
                    }
                }
            }
        });

        DeskHeightOverTimeChartConfig = JsonSerializer.Serialize(new
        {
            type = "line",
            data = new
            {
                labels = deskHeightOverTime.Keys,
                datasets = new[]
                {
                    new
                    {
                        label = "Average Desk Height",
                        data = deskHeightOverTime.Values,
                        backgroundColor = "rgba(255, 206, 86, 0.2)",
                        borderColor = "rgba(255, 206, 86, 1)",
                        borderWidth = 1
                    }
                }
            }
        });
    }
}
