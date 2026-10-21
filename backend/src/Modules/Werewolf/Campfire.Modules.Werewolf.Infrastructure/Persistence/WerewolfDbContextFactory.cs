using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace Campfire.Modules.Werewolf.Infrastructure.Persistence;

public sealed class WerewolfDbContextFactory : IDesignTimeDbContextFactory<WerewolfDbContext>
{
    public WerewolfDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Campfire") ?? "Host=localhost;Port=5433;Database=campfire;Username=campfire;Password=campfire-local-only";
        var options = new DbContextOptionsBuilder<WerewolfDbContext>().UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", WerewolfDbContext.Schema)).Options;
        return new WerewolfDbContext(options);
    }
}
