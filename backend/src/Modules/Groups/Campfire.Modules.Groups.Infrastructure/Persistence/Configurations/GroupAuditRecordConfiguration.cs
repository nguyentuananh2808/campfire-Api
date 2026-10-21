using Campfire.Modules.Groups.Domain.Auditing;
using Campfire.Modules.Groups.Domain.Groups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Groups.Infrastructure.Persistence.Configurations;

internal sealed class GroupAuditRecordConfiguration : IEntityTypeConfiguration<GroupAuditRecord>
{
    public void Configure(EntityTypeBuilder<GroupAuditRecord> builder)
    {
        builder.ToTable("GroupAuditRecords");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Action).HasMaxLength(50).IsRequired();
        builder.Property(record => record.TraceId).HasMaxLength(100).IsRequired();
        builder.Property(record => record.OccurredAtUtc).IsRequired();
        builder.HasIndex(record => new { record.GroupId, record.OccurredAtUtc });
        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(record => record.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
