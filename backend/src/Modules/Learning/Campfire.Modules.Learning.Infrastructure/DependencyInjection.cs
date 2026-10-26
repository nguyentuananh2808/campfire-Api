using Campfire.Modules.Learning.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Campfire.Modules.Learning.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddLearningModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Campfire") ?? throw new InvalidOperationException("Connection string 'Campfire' is not configured.");
        services.AddLearningApplication();
        services.AddSingleton(TimeProvider.System);
        services.AddDbContext<LearningDbContext>(options => options.UseNpgsql(connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", LearningDbContext.Schema)));
        services.AddScoped<ILearningService, EfLearningService>();
        services.AddHealthChecks().AddDbContextCheck<LearningDbContext>("learning-database");
        return services;
    }
}
