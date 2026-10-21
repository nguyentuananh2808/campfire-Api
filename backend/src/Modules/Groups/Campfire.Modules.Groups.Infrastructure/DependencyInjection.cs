using Campfire.Modules.Groups.Application;
using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Contracts;
using Campfire.Modules.Groups.Infrastructure.Persistence;
using Campfire.Modules.Groups.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.Modules.Groups.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGroupsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Campfire")
            ?? throw new InvalidOperationException("Connection string 'Campfire' is not configured.");

        services.AddGroupsApplication();
        services.AddDbContext<GroupsDbContext>(options => options.UseNpgsql(
            connectionString,
            postgres => postgres.MigrationsHistoryTable(
                "__EFMigrationsHistory",
                GroupsDbContext.Schema)));
        services.AddScoped<IGroupManagementService, EfGroupManagementService>();
        services.AddScoped<IGroupInvitationService, EfGroupInvitationService>();
        services.AddScoped<IGroupMembershipService, EfGroupMembershipService>();
        services.AddScoped<IGroupMembershipRevocationPublisher, GroupMembershipRevocationPublisher>();
        services.AddHealthChecks().AddDbContextCheck<GroupsDbContext>("groups-database");
        return services;
    }
}
