using Campfire.Modules.Werewolf.Application;
using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Infrastructure.Persistence;
using Campfire.Modules.Werewolf.Infrastructure.Realtime;
using Campfire.Modules.Werewolf.Infrastructure.Services;
using Campfire.Modules.Werewolf.Infrastructure.Voice;
using Campfire.Modules.Werewolf.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.Modules.Werewolf.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWerewolfModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Campfire") ?? throw new InvalidOperationException("ConnectionStrings:Campfire is required.");
        services.AddWerewolfApplication();
        services.AddDbContext<WerewolfDbContext>(options => options.UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", WerewolfDbContext.Schema)));
        services.AddHealthChecks().AddDbContextCheck<WerewolfDbContext>("werewolf-database");
        services.Configure<LiveKitOptions>(configuration.GetSection(LiveKitOptions.SectionName));
        services.AddScoped<IWerewolfService, EfWerewolfService>();
        services.AddSingleton<IWerewolfVoiceGateway, LiveKitVoiceGateway>();
        services.AddSingleton<IWerewolfNotificationPublisher, SignalRWerewolfNotificationPublisher>();
        services.AddSingleton<WerewolfConnectionRegistry>();
        services.AddSignalR(options => options.MaximumReceiveMessageSize = 16 * 1024);
        services.AddHostedService<WerewolfPhaseWorker>();
        return services;
    }
}
