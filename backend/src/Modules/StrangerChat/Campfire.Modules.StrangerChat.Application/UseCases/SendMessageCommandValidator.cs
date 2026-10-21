using FluentValidation;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand> { public SendMessageCommandValidator() { RuleFor(x => x.ClientMessageId).NotEmpty(); RuleFor(x => x.Content).NotEmpty().MaximumLength(1000).Must(x => !x.Contains("http://", StringComparison.OrdinalIgnoreCase) && !x.Contains("https://", StringComparison.OrdinalIgnoreCase) && !x.Contains("www.", StringComparison.OrdinalIgnoreCase)).WithMessage("Liên kết chưa được hỗ trợ."); } }
