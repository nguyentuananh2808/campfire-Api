using Campfire.Modules.Events.Domain.Auditing;
using Campfire.Modules.Events.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Events.Infrastructure.Persistence.Configurations;

public sealed class EventAuditRecordConfiguration : IEntityTypeConfiguration<EventAuditRecord>
{
    public void Configure(EntityTypeBuilder<EventAuditRecord> builder)
    {
        builder.ToTable("EventAuditRecords", EventsDbContext.Schema);
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Action).HasMaxLength(50).IsRequired();
        builder.Property(record => record.TraceId).HasMaxLength(100).IsRequired();
        builder.Property(record => record.OccurredAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(record => new { record.EventId, record.OccurredAtUtc });
        builder.HasOne<GroupEvent>().WithMany().HasForeignKey(record => record.EventId).OnDelete(DeleteBehavior.Cascade);
    }
}
