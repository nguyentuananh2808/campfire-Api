using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.CreateGroup;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.Modules.Groups.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddGroupsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateGroupCommandValidator>();
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining<CreateGroupCommand>();
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        return services;
    }
}
