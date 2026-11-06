using Campfire.Modules.Games.Application;
using Campfire.Modules.Games.Application.Abstractions;
using Campfire.Modules.Games.Infrastructure.Persistence;
using Campfire.Modules.Games.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.Modules.Games.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGamesModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Campfire")
            ?? throw new InvalidOperationException("Connection string 'Campfire' is not configured.");
        services.AddGamesApplication();
        services.AddDbContext<GamesDbContext>(options => options.UseNpgsql(connectionString, postgres =>
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", GamesDbContext.Schema)));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IGameService, EfGameService>();
        services.AddHealthChecks().AddDbContextCheck<GamesDbContext>("games-database");
        return services;
    }
}
