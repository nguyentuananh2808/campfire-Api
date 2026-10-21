using FluentValidation;
using Campfire.Modules.Chat.Domain.Messages;

namespace Campfire.Modules.Chat.Application.SendMessage;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(command => command.GroupId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.ClientMessageId).NotEmpty();
        RuleFor(command => command.Kind).NotEmpty().Must(kind => kind is "Text" or "Sticker")
            .WithMessage("Message kind must be Text or Sticker.");
        RuleFor(command => command.Content).NotNull().MaximumLength(2000);
        When(command => command.Kind is "Text", () =>
            RuleFor(command => command.Content).Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Text messages must contain a non-whitespace character."));
        When(command => command.Kind is "Sticker", () =>
            RuleFor(command => command.Content).Must(ChatStickerCatalog.Contains)
                .WithMessage("Sticker code is not supported."));
    }
}
