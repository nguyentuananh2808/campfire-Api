using Campfire.Modules.Werewolf.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Campfire.Modules.Werewolf.Infrastructure.Persistence.Configurations;

public sealed class GameEventConfiguration : IEntityTypeConfiguration<GameEvent>
{
    public void Configure(EntityTypeBuilder<GameEvent> builder)
    {
        builder.ToTable("GameEvents", WerewolfDbContext.Schema); builder.HasKey(x => x.Id); builder.Property(x => x.Kind).HasMaxLength(40); builder.Property(x => x.Message).HasMaxLength(500);
        builder.Property(x => x.OccurredAtUtc).HasColumnType("timestamp with time zone"); builder.HasIndex(x => new { x.GameId, x.OccurredAtUtc, x.Id });
        builder.HasOne<WerewolfGame>().WithMany().HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Cascade);
    }
}
