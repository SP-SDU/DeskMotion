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
using DeskMotion.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Pages;

public class DetailsModel(ApplicationDbContext context, RestRepository<int> restRepository, ILogger<DetailsModel> logger) : PageModel
{
    public DeskMetadata DeskMetadata { get; set; } = default!;
    public Desk LatestDeskData { get; set; } = default!;
    public List<string> DailyUsageLabels { get; set; } = [];
    public List<int> DailyUsageData { get; set; } = [];
    public List<string> StandingSittingLabels { get; set; } = ["Standing Time", "Sitting Time"];
    public List<int> StandingSittingData { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var deskMetadata = await context.DeskMetadata.FirstOrDefaultAsync(m => m.Id == id);
        if (deskMetadata == null)
        {
            return NotFound();
        }
        DeskMetadata = deskMetadata;

        var latestDeskData = await context.Desks
            .Where(d => d.MacAddress == DeskMetadata.MacAddress && d.IsLatest)
            .FirstOrDefaultAsync();

        if (latestDeskData == null)
        {
            return NotFound();
        }
        LatestDeskData = latestDeskData;

        var usageData = await context.Desks
            .Where(d => d.MacAddress == DeskMetadata.MacAddress && d.RecordedAt >= DateTime.UtcNow.AddDays(-7))
            .GroupBy(d => d.RecordedAt.Date)
            .Select(g => new { Date = g.Key, Usage = g.Sum(d => d.Usage.ActivationsCounter) })
            .ToListAsync();

        DailyUsageLabels = usageData.Select(u => u.Date.ToString("MM/dd")).ToList();
        DailyUsageData = usageData.Select(u => u.Usage).ToList();

        var standingData = await context.Desks
            .Where(d => d.MacAddress == DeskMetadata.MacAddress)
            .Select(d => d.State.Position_mm > 800)
            .ToListAsync();

        var total = standingData.Count;
        var standing = standingData.Count(s => s);

        StandingSittingData = [standing, total - standing];

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, int positionMm)
    {
        var deskMetadata = await context.DeskMetadata.FirstOrDefaultAsync(m => m.Id == id);
        if (deskMetadata == null)
        {
            return NotFound();
        }
        DeskMetadata = deskMetadata;

        var endpoint = $"desks/{DeskMetadata.MacAddress}/state";
        var payload = new { position_mm = positionMm };

        try
        {
            await restRepository.PutAsync(endpoint, payload);
            logger.LogInformation("Desk height adjusted to {PositionMm} mm successfully.", positionMm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to adjust desk height.");
        }

        return RedirectToPage(new { id });
    }
}
