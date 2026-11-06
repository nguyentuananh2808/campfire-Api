using Campfire.Modules.Games.Application.Common;
using Campfire.Modules.Games.Application.CreateGame;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.Modules.Games.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddGamesApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateGameCommandValidator>();
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining<CreateGameCommand>();
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        return services;
    }
}
