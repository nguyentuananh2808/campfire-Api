using Campfire.Modules.Werewolf.Domain.Games;
using Campfire.Modules.Werewolf.Domain.Rooms;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Werewolf.Infrastructure.Persistence;

public sealed class WerewolfDbContext(DbContextOptions<WerewolfDbContext> options) : DbContext(options)
{
    public const string Schema = "werewolf";
    public DbSet<WerewolfRoom> Rooms => Set<WerewolfRoom>();
    public DbSet<RoomPlayer> RoomPlayers => Set<RoomPlayer>();
    public DbSet<WerewolfGame> Games => Set<WerewolfGame>();
    public DbSet<GamePlayer> GamePlayers => Set<GamePlayer>();
    public DbSet<GameAction> Actions => Set<GameAction>();
    public DbSet<GameVote> Votes => Set<GameVote>();
    public DbSet<GameEvent> Events => Set<GameEvent>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(typeof(WerewolfDbContext).Assembly);
}
