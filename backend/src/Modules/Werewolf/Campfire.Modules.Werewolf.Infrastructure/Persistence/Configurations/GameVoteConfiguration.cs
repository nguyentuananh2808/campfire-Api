using Campfire.Modules.Werewolf.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Campfire.Modules.Werewolf.Infrastructure.Persistence.Configurations;

public sealed class GameVoteConfiguration : IEntityTypeConfiguration<GameVote>
{
    public void Configure(EntityTypeBuilder<GameVote> builder)
    {
        builder.ToTable("GameVotes", WerewolfDbContext.Schema); builder.HasKey(x => new { x.GameId, x.PhaseSequence, x.VoterUserId });
        builder.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone"); builder.HasIndex(x => new { x.GameId, x.VoterUserId, x.ClientActionId }).IsUnique();
        builder.HasOne<WerewolfGame>().WithMany().HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Cascade);
    }
}
