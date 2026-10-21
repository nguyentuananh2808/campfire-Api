using Campfire.Modules.Chat.Infrastructure.Persistence;
using Campfire.Modules.Events.Infrastructure.Persistence;
using Campfire.Modules.Groups.Infrastructure.Persistence;
using Campfire.Modules.Identity.Infrastructure.Persistence;
using Campfire.Modules.StrangerChat.Infrastructure.Persistence;
using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Campfire.IntegrationTests;

public sealed class CampfireApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public TestTimeProvider Clock { get; } = new(DateTimeOffset.Parse("2026-10-16T00:00:00Z"));

    private readonly PostgreSqlContainer database = new PostgreSqlBuilder("postgres:18")
        .WithDatabase("campfire_identity_tests")
        .WithUsername("campfire")
        .WithPassword("integration-test-only")
        .Build();

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await dbContext.Database.MigrateAsync();
        var groupsDbContext = scope.ServiceProvider.GetRequiredService<GroupsDbContext>();
        await groupsDbContext.Database.MigrateAsync();
        var chatDbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        await chatDbContext.Database.MigrateAsync();
        var eventsDbContext = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
        await eventsDbContext.Database.MigrateAsync();
        var werewolfDbContext = scope.ServiceProvider.GetRequiredService<WerewolfDbContext>();
        await werewolfDbContext.Database.MigrateAsync();
        var strangerChatDbContext = scope.ServiceProvider.GetRequiredService<StrangerChatDbContext>();
        await strangerChatDbContext.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Dispose();
        await database.DisposeAsync();
    }

    public async Task AdvanceWerewolfAsync(TimeSpan duration)
    {
        Clock.Advance(duration);
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IWerewolfService>().AdvanceDueGamesAsync(CancellationToken.None);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Campfire", database.GetConnectionString());
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Campfire"] = database.GetConnectionString(),
                ["LiveKit:PublicUrl"] = "ws://livekit.integration.test",
                ["LiveKit:ApiUrl"] = "http://livekit.integration.test",
                ["LiveKit:ApiKey"] = "integration-test-key",
                ["LiveKit:ApiSecret"] = "integration-test-secret-with-32-bytes",
                ["LiveKit:TokenLifetimeSeconds"] = "60",
                ["RateLimits:StrangerSessionPermitLimit"] = "100"
            }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }
}
