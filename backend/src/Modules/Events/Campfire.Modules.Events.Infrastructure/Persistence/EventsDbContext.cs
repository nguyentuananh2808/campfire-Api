using Campfire.Modules.Events.Domain.Auditing;
using Campfire.Modules.Events.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Events.Infrastructure.Persistence;

public sealed class EventsDbContext(DbContextOptions<EventsDbContext> options) : DbContext(options)
{
    public const string Schema = "events";
    public DbSet<GroupEvent> Events => Set<GroupEvent>();
    public DbSet<EventRsvp> Rsvps => Set<EventRsvp>();
    public DbSet<EventAuditRecord> AuditRecords => Set<EventAuditRecord>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventsDbContext).Assembly);
}
