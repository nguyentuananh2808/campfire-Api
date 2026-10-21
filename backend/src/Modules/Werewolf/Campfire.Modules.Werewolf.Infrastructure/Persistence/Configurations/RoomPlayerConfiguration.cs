using Campfire.Modules.Werewolf.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Campfire.Modules.Werewolf.Infrastructure.Persistence.Configurations;

public sealed class RoomPlayerConfiguration : IEntityTypeConfiguration<RoomPlayer>
{
    public void Configure(EntityTypeBuilder<RoomPlayer> builder)
    {
        builder.ToTable("RoomPlayers", WerewolfDbContext.Schema); builder.HasKey(x => new { x.RoomId, x.UserId });
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(12); builder.Property(x => x.JoinedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.LastSeenAtUtc).HasColumnType("timestamp with time zone");
        builder.HasOne<WerewolfRoom>().WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade); builder.HasIndex(x => x.UserId);
    }
}
