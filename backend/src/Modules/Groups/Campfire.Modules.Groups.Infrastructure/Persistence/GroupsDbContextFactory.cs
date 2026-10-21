using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Campfire.Modules.Groups.Infrastructure.Persistence;

public sealed class GroupsDbContextFactory : IDesignTimeDbContextFactory<GroupsDbContext>
{
    public GroupsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Campfire");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set ConnectionStrings__Campfire before creating or applying Groups migrations.");
        }

        var options = new DbContextOptionsBuilder<GroupsDbContext>()
            .UseNpgsql(connectionString, postgres => postgres.MigrationsHistoryTable(
                "__EFMigrationsHistory",
                GroupsDbContext.Schema))
            .Options;
        return new GroupsDbContext(options);
    }
}
