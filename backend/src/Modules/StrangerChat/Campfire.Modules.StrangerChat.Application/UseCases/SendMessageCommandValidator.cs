using Campfire.Modules.StrangerChat.Domain.Entities;
using FluentValidation;

namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(command => command.ClientMessageId).NotEmpty();
        RuleFor(command => command.Kind).Must(kind => kind is "Text" or "Sticker")
            .WithMessage("Message kind must be Text or Sticker.");
        RuleFor(command => command.Content).NotNull().MaximumLength(1000);
        When(command => command.Kind is "Text", () =>
            RuleFor(command => command.Content)
                .Must(content => !string.IsNullOrWhiteSpace(content))
                .Must(content => !content.Contains("http://", StringComparison.OrdinalIgnoreCase)
                    && !content.Contains("https://", StringComparison.OrdinalIgnoreCase)
                    && !content.Contains("www.", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Liên kết chưa được hỗ trợ."));
        When(command => command.Kind is "Sticker", () =>
            RuleFor(command => command.Content).Must(StrangerStickerCatalog.Contains)
                .WithMessage("Sticker code is not supported."));
    }
}
