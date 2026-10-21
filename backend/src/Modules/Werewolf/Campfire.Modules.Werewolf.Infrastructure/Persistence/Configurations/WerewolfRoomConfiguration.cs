using Campfire.Modules.Werewolf.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Campfire.Modules.Werewolf.Infrastructure.Persistence.Configurations;

public sealed class WerewolfRoomConfiguration : IEntityTypeConfiguration<WerewolfRoom>
{
    public void Configure(EntityTypeBuilder<WerewolfRoom> builder)
    {
        builder.ToTable("Rooms", WerewolfDbContext.Schema); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(8).IsRequired(); builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(80).IsRequired(); builder.Property(x => x.Visibility).HasConversion<string>().HasMaxLength(12);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(16); builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone"); builder.Property(x => x.UpdatedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.Visibility, x.Status, x.CreatedAtUtc });
    }
}
