using Campfire.Modules.Werewolf.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Campfire.Modules.Werewolf.Infrastructure.Persistence.Configurations;

public sealed class GameActionConfiguration : IEntityTypeConfiguration<GameAction>
{
    public void Configure(EntityTypeBuilder<GameAction> builder)
    {
        builder.ToTable("GameActions", WerewolfDbContext.Schema); builder.HasKey(x => x.Id); builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone"); builder.HasIndex(x => new { x.GameId, x.ActorUserId, x.ClientActionId }).IsUnique();
        builder.HasIndex(x => new { x.GameId, x.PhaseSequence, x.ActorUserId }).IsUnique(); builder.HasOne<WerewolfGame>().WithMany().HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Cascade);
    }
}
