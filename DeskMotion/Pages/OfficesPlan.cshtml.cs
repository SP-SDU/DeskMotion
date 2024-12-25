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
    public OfficesPlan OfficesPlan { get; set; } = new OfficesPlan();

    public async Task<IActionResult> OnGetAsync()
    {
        var existingPlan = await _context.OfficesPlan.FirstOrDefaultAsync();
        if (existingPlan != null)
        {
            OfficesPlan = existingPlan;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(OfficesPlan.BgCanvasData) || string.IsNullOrEmpty(OfficesPlan.FgCanvasData))
        {
            ModelState.AddModelError("", "Canvas data cannot be empty.");
            return Page();
        }

        try
        {
            // Validate JSON
            JsonDocument.Parse(OfficesPlan.FgCanvasData);
        }
        catch (JsonException)
        {
            ModelState.AddModelError("", "Invalid JSON format in FgCanvasData.");
            return Page();
        }

        var existingPlan = await _context.OfficesPlan.FirstOrDefaultAsync();
        if (existingPlan != null)
        {
            existingPlan.BgCanvasData = OfficesPlan.BgCanvasData;
            existingPlan.FgCanvasData = OfficesPlan.FgCanvasData;
        }
        else
        {
            _context.OfficesPlan.Add(OfficesPlan);
        }

        await _context.SaveChangesAsync();
        return Page();
    }
}
