using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.SendMessage;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.Modules.Chat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddChatApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SendMessageCommandValidator>();
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining<SendMessageCommand>();
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        return services;
    }
}
