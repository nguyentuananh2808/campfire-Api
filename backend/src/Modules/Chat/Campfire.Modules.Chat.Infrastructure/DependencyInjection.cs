using Campfire.Modules.Chat.Application;
using Campfire.Modules.Chat.Application.Abstractions;
using Campfire.Modules.Chat.Infrastructure.Persistence;
using Campfire.Modules.Chat.Infrastructure.Realtime;
using Campfire.Modules.Chat.Infrastructure.Services;
using Campfire.Modules.Groups.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.Modules.Chat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddChatModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Campfire")
            ?? throw new InvalidOperationException("Connection string 'Campfire' is not configured.");
        services.AddChatApplication();
        services.AddDbContext<ChatDbContext>(options => options.UseNpgsql(connectionString, postgres =>
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", ChatDbContext.Schema)));
        services.AddScoped<IChatMessageService, EfChatMessageService>();
        services.AddScoped<IChatNotificationPublisher, SignalRChatNotificationPublisher>();
        services.AddSingleton<ChatConnectionRegistry>();
        services.AddScoped<IGroupMembershipRevocationListener, ChatMembershipRevocationListener>();
        services.AddSignalR(options => options.MaximumReceiveMessageSize = 16 * 1024);
        services.AddHealthChecks().AddDbContextCheck<ChatDbContext>("chat-database");
        return services;
    }
}
