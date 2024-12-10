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
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<IssueReport> IssueReports => Set<IssueReport>(); // Pabcf
}
