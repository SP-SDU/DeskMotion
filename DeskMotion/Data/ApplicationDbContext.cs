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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Desk> Desks => Set<Desk>();
    public DbSet<DeskInfo> DeskInfos => Set<DeskInfo>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<DeskPreference> DeskPreferences => Set<DeskPreference>();
    public DbSet<ScheduledHeight> ScheduledHeights => Set<ScheduledHeight>();
    public DbSet<HeightAdjustment> HeightAdjustments => Set<HeightAdjustment>();
    public DbSet<DeskPreset> DeskPresets => Set<DeskPreset>();
    public DbSet<UsageSession> UsageSessions => Set<UsageSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships
        builder.Entity<DeskInfo>()
            .HasMany(d => d.Reservations)
            .WithOne()
            .HasForeignKey(r => r.DeskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DeskInfo>()
            .HasMany(d => d.HeightAdjustments)
            .WithOne()
            .HasForeignKey(h => h.DeskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DeskInfo>()
            .HasMany(d => d.Preferences)
            .WithOne(p => p.Desk)
            .HasForeignKey(p => p.DeskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DeskPreference>()
            .HasMany(p => p.ScheduledHeights)
            .WithOne(h => h.Preference)
            .HasForeignKey(h => h.PreferenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
