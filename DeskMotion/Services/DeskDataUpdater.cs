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
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Services;

public class DeskDataUpdater(IServiceProvider serviceProvider, ILogger<DeskDataUpdater> logger) : BackgroundService
{
    private bool _isUpdating = false;
    private readonly TimeSpan _delay = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DeskDataUpdater started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_isUpdating)
            {
                await Task.Delay(_delay, stoppingToken);
                continue;
            }

            _isUpdating = true;

            try
            {
                using var scope = serviceProvider.CreateScope();
                var deskIdsRepository = scope.ServiceProvider.GetRequiredService<RestRepository<List<string>>>();
                var deskIds = await deskIdsRepository.GetAsync("desks");

                var tasks = deskIds.Select(deskId => ProcessDeskAsync(deskId, stoppingToken));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating desk data");
            }
            finally
            {
                _isUpdating = false;
            }

            await Task.Delay(_delay, stoppingToken);
        }

        logger.LogInformation("DeskDataUpdater stopping.");
    }

    private async Task ProcessDeskAsync(string deskId, CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deskRepository = scope.ServiceProvider.GetRequiredService<RestRepository<Desk>>();

        // Ensure metadata exists
        if (!await dbContext.DeskMetadata.AnyAsync(dm => dm.MacAddress == deskId, stoppingToken))
        {
            _ = dbContext.DeskMetadata.Add(new DeskMetadata { MacAddress = deskId });
            _ = await dbContext.SaveChangesAsync(stoppingToken);
        }

        // Fetch desk data
        var deskData = await deskRepository.GetAsync($"desks/{deskId}");
        if (deskData == null) return;

        deskData.MacAddress = deskId;
        deskData.LastErrors ??= [];

        // Update desk data
        var currentLatest = await dbContext.Desks.FirstOrDefaultAsync(d => d.MacAddress == deskId && d.IsLatest, stoppingToken);
        if (currentLatest != null)
        {
            currentLatest.IsLatest = false;
            _ = dbContext.Desks.Update(currentLatest);
        }

        deskData.IsLatest = true;
        _ = dbContext.Desks.Add(deskData);
        _ = await dbContext.SaveChangesAsync(stoppingToken);
    }
}
