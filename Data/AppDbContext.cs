using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Turnos.Models;

namespace Turnos.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Role> StaffRoles => Set<Role>();
    public DbSet<PersonRole> PersonRoles => Set<PersonRole>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<MessageLog> MessageLogs => Set<MessageLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Record> Records => Set<Record>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PersonRole>()
            .HasKey(pr => new { pr.PersonId, pr.RoleId });

        builder.Entity<PersonRole>()
            .HasOne(pr => pr.Person)
            .WithMany(p => p.PersonRoles)
            .HasForeignKey(pr => pr.PersonId);

        builder.Entity<PersonRole>()
            .HasOne(pr => pr.Role)
            .WithMany(r => r.PersonRoles)
            .HasForeignKey(pr => pr.RoleId);

        builder.Entity<Assignment>()
            .HasIndex(a => new { a.PersonId, a.StartDateTime, a.EndDateTime });

        builder.Entity<Event>()
            .HasIndex(e => e.StartDateTime);

        builder.Entity<Event>()
            .HasOne(e => e.Location)
            .WithMany(l => l.Events)
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<AuditLog>()
            .HasIndex(a => new { a.EntityType, a.EntityId });

        builder.Entity<MessageLog>()
            .HasOne(m => m.Event)
            .WithMany(e => e.MessageLogs)
            .HasForeignKey(m => m.EventId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
