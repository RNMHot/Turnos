using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<AttendanceBreak> AttendanceBreaks => Set<AttendanceBreak>();
    public DbSet<MessageLog> MessageLogs => Set<MessageLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Record> Records => Set<Record>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<WhatsAppGroup> WhatsAppGroups => Set<WhatsAppGroup>();
    public DbSet<LocationPosition> LocationPositions => Set<LocationPosition>();
    public DbSet<EventComment> EventComments => Set<EventComment>();
    public DbSet<EventContract> EventContracts => Set<EventContract>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppSetting>().HasKey(s => s.Key);

        builder.Entity<Person>().HasQueryFilter(p => !p.Deleted);
        builder.Entity<Company>().HasQueryFilter(c => !c.Deleted);
        builder.Entity<Event>().HasQueryFilter(e => !e.Deleted);
        builder.Entity<Location>().HasQueryFilter(l => !l.Deleted);
        builder.Entity<Role>().HasQueryFilter(r => !r.Deleted);
        builder.Entity<Assignment>().HasQueryFilter(a => !a.Deleted);
        builder.Entity<Attendance>().HasQueryFilter(a => !a.Deleted);
        builder.Entity<AttendanceBreak>().HasQueryFilter(b => !b.Deleted);
        builder.Entity<PersonRole>().HasQueryFilter(pr => !pr.Deleted);
        builder.Entity<Availability>().HasQueryFilter(a => !a.Deleted);
        builder.Entity<AuditLog>().HasQueryFilter(l => !l.Deleted);
        builder.Entity<MessageLog>().HasQueryFilter(m => !m.Deleted);
        builder.Entity<Record>().HasQueryFilter(r => !r.Deleted);

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

        builder.Entity<Attendance>()
            .HasIndex(a => new { a.PersonId, a.CheckInDateTime });

        builder.Entity<Attendance>()
            .HasIndex(a => new { a.EventId, a.CheckInDateTime });

        builder.Entity<Attendance>()
            .HasOne(a => a.Person)
            .WithMany(p => p.Attendances)
            .HasForeignKey(a => a.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Attendance>()
            .HasOne(a => a.Event)
            .WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AttendanceBreak>()
            .HasOne(b => b.Attendance)
            .WithMany(a => a.Breaks)
            .HasForeignKey(b => b.AttendanceId)
            .OnDelete(DeleteBehavior.Cascade);

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

        builder.Entity<MessageLog>()
            .HasOne(m => m.WhatsAppGroup)
            .WithMany(g => g.MessageLogs)
            .HasForeignKey(m => m.WhatsAppGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<WhatsAppGroup>().HasQueryFilter(g => !g.Deleted);
        builder.Entity<LocationPosition>().HasQueryFilter(p => !p.Deleted);
        builder.Entity<EventComment>().HasQueryFilter(c => !c.Deleted);

        builder.Entity<EventComment>()
            .HasIndex(c => new { c.EventId, c.CreatedAt });

        builder.Entity<EventComment>()
            .HasOne(c => c.Event)
            .WithMany()
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EventComment>()
            .HasOne(c => c.Person)
            .WithMany()
            .HasForeignKey(c => c.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EventContract>()
            .HasIndex(c => c.EventId)
            .IsUnique();

        builder.Entity<EventContract>()
            .HasOne(c => c.Event)
            .WithOne()
            .HasForeignKey<EventContract>(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<LocationPosition>()
            .HasOne(p => p.Location)
            .WithMany(l => l.Positions)
            .HasForeignKey(p => p.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Assignment>()
            .HasOne(a => a.LocationPosition)
            .WithMany(p => p.Assignments)
            .HasForeignKey(a => a.LocationPositionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Assignment>()
            .HasOne(a => a.Role)
            .WithMany()
            .HasForeignKey(a => a.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        // SQL Server returns DateTime without Kind; mark all as UTC so ToLocalTime() converts correctly.
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        var utcNullableConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v,
            v => v == null ? null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(utcNullableConverter);
            }
        }
    }
}
