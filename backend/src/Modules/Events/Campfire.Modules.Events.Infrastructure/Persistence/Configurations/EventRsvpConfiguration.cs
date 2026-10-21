using Campfire.Modules.Events.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Events.Infrastructure.Persistence.Configurations;

public sealed class EventRsvpConfiguration : IEntityTypeConfiguration<EventRsvp>
{
    public void Configure(EntityTypeBuilder<EventRsvp> builder)
    {
        builder.ToTable("EventRsvps", EventsDbContext.Schema);
        builder.HasKey(rsvp => new { rsvp.EventId, rsvp.UserId });
        builder.Property(rsvp => rsvp.Response).HasConversion<string>().HasMaxLength(12).IsRequired();
        builder.Property(rsvp => rsvp.RespondedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasOne<GroupEvent>().WithMany().HasForeignKey(rsvp => rsvp.EventId).OnDelete(DeleteBehavior.Cascade);
    }
}
