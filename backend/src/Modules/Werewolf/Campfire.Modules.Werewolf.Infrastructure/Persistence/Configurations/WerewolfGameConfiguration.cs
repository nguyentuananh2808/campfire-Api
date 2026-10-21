using Campfire.Modules.Werewolf.Domain.Games;
using Campfire.Modules.Werewolf.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Campfire.Modules.Werewolf.Infrastructure.Persistence.Configurations;

public sealed class WerewolfGameConfiguration : IEntityTypeConfiguration<WerewolfGame>
{
    public void Configure(EntityTypeBuilder<WerewolfGame> builder)
    {
        builder.ToTable("Games", WerewolfDbContext.Schema); builder.HasKey(x => x.Id); builder.HasIndex(x => x.RoomId).IsUnique();
        builder.Property(x => x.Phase).HasConversion<string>().HasMaxLength(24); builder.Property(x => x.Winner).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Version).IsConcurrencyToken(); builder.Property(x => x.PhaseEndsAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.StartedAtUtc).HasColumnType("timestamp with time zone"); builder.Property(x => x.FinishedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.Phase, x.PhaseEndsAtUtc }); builder.HasOne<WerewolfRoom>().WithOne().HasForeignKey<WerewolfGame>(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
    }
}
