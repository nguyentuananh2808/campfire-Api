using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
namespace Campfire.Modules.Learning.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddLearningApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<GetMindMapQueryValidator>();
        services.AddMediatR(c => { c.RegisterServicesFromAssemblyContaining<GetTodayQuery>(); c.AddOpenBehavior(typeof(ValidationBehavior<,>)); });
        return services;
    }
}
