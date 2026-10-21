using Microsoft.EntityFrameworkCore; using Microsoft.EntityFrameworkCore.Design;
namespace Campfire.Modules.StrangerChat.Infrastructure.Persistence;
public sealed class StrangerChatDbContextFactory : IDesignTimeDbContextFactory<StrangerChatDbContext>
{
    public StrangerChatDbContext CreateDbContext(string[] args) { var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Campfire") ?? throw new InvalidOperationException("Set ConnectionStrings__Campfire."); var options = new DbContextOptionsBuilder<StrangerChatDbContext>().UseNpgsql(cs, x => x.MigrationsHistoryTable("__EFMigrationsHistory", StrangerChatDbContext.Schema)).Options; return new StrangerChatDbContext(options); }
}
