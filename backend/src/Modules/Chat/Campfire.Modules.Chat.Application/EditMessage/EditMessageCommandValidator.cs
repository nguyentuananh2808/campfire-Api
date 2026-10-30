using FluentValidation;

namespace Campfire.Modules.Chat.Application.EditMessage;

public sealed class EditMessageCommandValidator : AbstractValidator<EditMessageCommand>
{
    public EditMessageCommandValidator()
    {
        RuleFor(command => command.GroupId).NotEmpty();
        RuleFor(command => command.MessageId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.ExpectedVersion).NotEmpty();
        RuleFor(command => command.Content).NotEmpty().MaximumLength(2000).Must(content => !string.IsNullOrWhiteSpace(content));
    }
}
