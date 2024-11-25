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

namespace DeskMotion.Services;

public class DeskDataUpdater(DeskService deskService, IServiceProvider serviceProvider, ILogger<DeskDataUpdater> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Updating desk data...");

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                var deskIds = await deskService.GetDeskIdsAsync();

                foreach (var deskId in deskIds)
                {
                    var deskData = await deskService.GetDeskAsync(deskId);

                    // Update or add to database
                    var existingDesk = dbContext.Desks.FirstOrDefault(d => d.MacAddress == deskId);
                    if (existingDesk != null)
                    {
                        existingDesk.MacAddress = deskData.MacAddress;
                        existingDesk.Config = deskData.Config;
                        existingDesk.State = deskData.State;
                        existingDesk.Usage = deskData.Usage;
                        existingDesk.LastErrors = deskData.LastErrors;
                    }
                    else
                    {
                        _ = dbContext.Desks.Add(deskData);
                    }
                }

                _ = await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating desk data");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
