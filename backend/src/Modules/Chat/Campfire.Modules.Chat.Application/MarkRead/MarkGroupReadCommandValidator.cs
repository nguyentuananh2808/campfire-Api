using FluentValidation;

namespace Campfire.Modules.Chat.Application.MarkRead;

public sealed class MarkGroupReadCommandValidator : AbstractValidator<MarkGroupReadCommand>
{
    public MarkGroupReadCommandValidator()
    {
        RuleFor(command => command.GroupId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.MessageId).NotEmpty();
    }
}
