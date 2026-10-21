using Campfire.Modules.Identity.Application.Common;
using Campfire.Modules.Identity.Application.Register;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.Modules.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining<RegisterCommand>();
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        return services;
    }
}
