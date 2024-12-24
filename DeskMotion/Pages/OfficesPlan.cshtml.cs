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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DeskMotion.Pages;

public class OfficesPlanModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public OfficesPlanModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public OfficesPlan? OfficesPlan { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        OfficesPlan = await _context.OfficesPlan.FirstOrDefaultAsync();

        if (OfficesPlan == null)
        {
            OfficesPlan = new OfficesPlan();
            _context.OfficesPlan.Add(OfficesPlan);
            await _context.SaveChangesAsync();
        }

        return Page();
    }

    // Handler for fetching saved data (called by JavaScript)
    public async Task<IActionResult> OnGetSavedDataAsync()
    {
        var plan = await _context.OfficesPlan.FirstOrDefaultAsync();
        if (plan == null)
        {
            return new JsonResult(new { success = false, message = "No data found" });
        }

        return new JsonResult(plan);
    }

    // Handler for saving data (called by JavaScript)
    public async Task<IActionResult> OnPostSaveDataAsync([FromBody] OfficesPlanDto planData)
    {
        if (planData == null || string.IsNullOrEmpty(planData.FgCanvasData) || string.IsNullOrEmpty(planData.BgCanvasData))
        {
            return BadRequest(new { success = false, message = "Invalid or missing data." });
        }

        var existingPlan = await _context.OfficesPlan.FirstOrDefaultAsync();
        if (existingPlan != null)
        {
            existingPlan.BgCanvasData = planData.BgCanvasData;
            existingPlan.FgCanvasData = planData.FgCanvasData;
        }
        else
        {
            var newPlan = new OfficesPlan
            {
                BgCanvasData = planData.BgCanvasData,
                FgCanvasData = planData.FgCanvasData
            };
            _context.OfficesPlan.Add(newPlan);
        }

        await _context.SaveChangesAsync();

        return new JsonResult(new { success = true });
    }
}
public class OfficesPlanDto
{
    public string BgCanvasData { get; set; } = string.Empty;
    public string FgCanvasData { get; set; } = string.Empty;
}
