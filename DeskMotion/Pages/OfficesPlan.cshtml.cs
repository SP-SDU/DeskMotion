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

namespace DeskMotion.Pages;

public class OfficesPlanModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public OfficesPlanModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public OfficesPlan OfficesPlan { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var officesPlan = await _context.OfficesPlan.FirstOrDefaultAsync(p => p.Id == id);

        if (OfficesPlan == null)
        {
            return NotFound();
        }
        else
        {
            officesPlan = OfficesPlan;
        }

        return Page();
    }

    public async Task<IActionResult> OnGetSavedData(Guid? id)
    {
        var officesPlan = await _context.OfficesPlan.FirstOrDefaultAsync(p => p.Id == id);
        if (officesPlan != null)
        {
            return new JsonResult(officesPlan);
        }
        return new JsonResult(null);
    }

    public async Task<IActionResult> OnPostSaveData(Guid? id, string bgCanvasData, string fgCanvasData)
    {

        var existingPlan = await _context.OfficesPlan.FirstOrDefaultAsync(p => p.Id == id);
        if (existingPlan == null)
        {
            existingPlan = new OfficesPlan
            {
                FgCanvasData = fgCanvasData,
                BgCanvasData = bgCanvasData
            };
            _context.OfficesPlan.Add(existingPlan);
        }

            OfficesPlan.BgCanvasData = bgCanvasData;
            OfficesPlan.FgCanvasData = fgCanvasData;
            await _context.SaveChangesAsync();

        return new JsonResult(new { success = true });
    }
}

