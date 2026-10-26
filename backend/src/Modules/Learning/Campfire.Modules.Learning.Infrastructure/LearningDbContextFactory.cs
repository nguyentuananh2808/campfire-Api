using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace Campfire.Modules.Learning.Infrastructure;
public sealed class LearningDbContextFactory : IDesignTimeDbContextFactory<LearningDbContext>
{
    public LearningDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Campfire") ?? "Host=localhost;Port=5433;Database=campfire;Username=campfire;Password=campfire_dev_only";
        return new(new DbContextOptionsBuilder<LearningDbContext>().UseNpgsql(connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", LearningDbContext.Schema)).Options);
    }
}
