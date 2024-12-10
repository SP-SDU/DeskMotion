using DeskMotion.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeskMotion.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, Role, Guid>(options)
{
    public DbSet<Desk> Desks => Set<Desk>();
    public DbSet<DeskMetadata> DeskMetadata => Set<DeskMetadata>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<IssueReport> IssueReports => Set<IssueReport>(); // Pabcf
}
