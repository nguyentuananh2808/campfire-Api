using Campfire.Modules.Events.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Events.Infrastructure.Persistence.Configurations;

public sealed class GroupEventConfiguration : IEntityTypeConfiguration<GroupEvent>
{
    public void Configure(EntityTypeBuilder<GroupEvent> builder)
    {
        builder.ToTable("Events", EventsDbContext.Schema);
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Title).HasMaxLength(120).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(2000).IsRequired();
        builder.Property(item => item.Location).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(12).IsRequired();
        builder.Property(item => item.StartsAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(item => item.CreatedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(item => item.UpdatedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(item => item.Version).IsConcurrencyToken();
        builder.HasIndex(item => new { item.GroupId, item.StartsAtUtc, item.Id });
    }
}
