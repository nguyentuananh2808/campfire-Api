using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Campfire.Modules.Games.Infrastructure.Persistence;

public sealed class GamesDbContextFactory : IDesignTimeDbContextFactory<GamesDbContext>
{
    public GamesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Campfire");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Set ConnectionStrings__Campfire before creating or applying Games migrations.");
        var options = new DbContextOptionsBuilder<GamesDbContext>()
            .UseNpgsql(connectionString, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", GamesDbContext.Schema))
            .Options;
        return new GamesDbContext(options);
    }
}
