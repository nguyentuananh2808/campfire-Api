using Campfire.Modules.Events.Application;
using Campfire.Modules.Events.Application.Abstractions;
using Campfire.Modules.Events.Infrastructure.Persistence;
using Campfire.Modules.Events.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.Modules.Events.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEventsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Campfire")
            ?? throw new InvalidOperationException("Connection string 'Campfire' is not configured.");
        services.AddEventsApplication();
        services.AddDbContext<EventsDbContext>(options => options.UseNpgsql(connectionString, postgres =>
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", EventsDbContext.Schema)));
        services.AddScoped<IEventService, EfEventService>();
        services.AddHealthChecks().AddDbContextCheck<EventsDbContext>("events-database");
        return services;
    }
}
