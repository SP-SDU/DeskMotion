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

using DeskMotion.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Data;

public static class DbInitializer
{
    /// <summary> Seeding the database. </summary>
    public static void Initialize(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        using ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<Role>>();

        // Apply any pending migrations
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }

        // Ensure roles are created
        var roles = new[] { "Administrator", "User" };
        foreach (var role in roles)
        {
            if (!roleManager.RoleExistsAsync(role).Result)
            {
                roleManager.CreateAsync(new Role { Name = role }).Wait();
            }
        }

        // Find or create admin user
        var adminEmail = "admin@example.com";
        var adminUser = userManager.FindByEmailAsync(adminEmail).Result;
        
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User"
            };
            var result = userManager.CreateAsync(adminUser, "Admin123!").Result;
            if (result.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, "Administrator").Wait();
            }
        }
        else
        {
            // Reset password for existing admin
            var token = userManager.GeneratePasswordResetTokenAsync(adminUser).Result;
            var resetResult = userManager.ResetPasswordAsync(adminUser, token, "Admin123!").Result;
        }

        context.SaveChanges();
    }
}
