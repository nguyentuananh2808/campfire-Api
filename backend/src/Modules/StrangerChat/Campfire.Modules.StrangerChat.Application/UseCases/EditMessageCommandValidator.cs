using FluentValidation;

namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed class EditMessageCommandValidator : AbstractValidator<EditMessageCommand>
{
    public EditMessageCommandValidator()
    {
        RuleFor(command => command.Token).NotEmpty();
        RuleFor(command => command.ConversationId).NotEmpty();
        RuleFor(command => command.MessageId).NotEmpty();
        RuleFor(command => command.ExpectedVersion).NotEmpty();
        RuleFor(command => command.Content).NotEmpty().MaximumLength(1000).Must(content => !string.IsNullOrWhiteSpace(content));
    }
}
