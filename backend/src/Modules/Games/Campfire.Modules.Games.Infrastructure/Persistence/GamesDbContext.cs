using Campfire.Modules.Games.Domain.Caro;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Games.Infrastructure.Persistence;

public sealed class GamesDbContext(DbContextOptions<GamesDbContext> options) : DbContext(options)
{
    public const string Schema = "games";
    public DbSet<CaroGame> CaroGames => Set<CaroGame>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GamesDbContext).Assembly);
}
