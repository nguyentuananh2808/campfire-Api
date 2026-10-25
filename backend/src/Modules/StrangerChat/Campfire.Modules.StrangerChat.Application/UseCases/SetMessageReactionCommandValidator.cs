using Campfire.Modules.StrangerChat.Domain.Entities;
using FluentValidation;

namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed class SetMessageReactionCommandValidator : AbstractValidator<SetMessageReactionCommand>
{
    public SetMessageReactionCommandValidator()
    {
        RuleFor(command => command.ConversationId).NotEmpty();
        RuleFor(command => command.MessageId).NotEmpty();
        RuleFor(command => command.Reaction)
            .Must(reaction => reaction is null || StrangerReactionCatalog.Contains(reaction))
            .WithMessage("Reaction code is not supported.");
    }
}
