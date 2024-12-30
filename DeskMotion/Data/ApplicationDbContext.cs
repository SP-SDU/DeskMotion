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

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, Role, Guid>(options)
{
    public DbSet<Desk> Desks => Set<Desk>();
    public DbSet<DeskMetadata> DeskMetadata => Set<DeskMetadata>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<IssueReport> IssueReports => Set<IssueReport>();
    public DbSet<OfficesPlan> OfficesPlan => Set<OfficesPlan>();
    public DbSet<InitialData> InitialData => Set<InitialData>();
}
