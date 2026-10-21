using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.CreateEvent;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.Modules.Events.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddEventsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateEventCommandValidator>();
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining<CreateEventCommand>();
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        return services;
    }
}
