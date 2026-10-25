using Campfire.Modules.Chat.Domain.Messages;
using FluentValidation;

namespace Campfire.Modules.Chat.Application.SetReaction;

public sealed class SetMessageReactionCommandValidator : AbstractValidator<SetMessageReactionCommand>
{
    public SetMessageReactionCommandValidator()
    {
        RuleFor(command => command.GroupId).NotEmpty();
        RuleFor(command => command.MessageId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Reaction).Must(reaction => reaction is null || ChatReactionCatalog.Contains(reaction))
            .WithMessage("Reaction code is not supported.");
    }
}
