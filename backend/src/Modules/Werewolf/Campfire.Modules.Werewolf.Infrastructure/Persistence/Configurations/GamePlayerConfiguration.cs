using Campfire.Modules.Werewolf.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Campfire.Modules.Werewolf.Infrastructure.Persistence.Configurations;

public sealed class GamePlayerConfiguration : IEntityTypeConfiguration<GamePlayer>
{
    public void Configure(EntityTypeBuilder<GamePlayer> builder)
    {
        builder.ToTable("GamePlayers", WerewolfDbContext.Schema); builder.HasKey(x => new { x.GameId, x.UserId });
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(16); builder.HasIndex(x => new { x.GameId, x.Seat }).IsUnique();
        builder.HasOne<WerewolfGame>().WithMany().HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Cascade);
    }
}
