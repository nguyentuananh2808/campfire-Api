using FluentValidation;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.GetMindMap;
using Campfire.Modules.Learning.Application.GetToday;
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
