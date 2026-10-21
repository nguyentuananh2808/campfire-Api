using Campfire.Modules.Werewolf.Domain.Games;
using FluentValidation;
namespace Campfire.Modules.Werewolf.Application.SubmitAction;

public sealed class SubmitActionCommandValidator : AbstractValidator<SubmitActionCommand>
{ public SubmitActionCommandValidator() { RuleFor(x => x.ClientActionId).NotEmpty(); RuleFor(x => x.Kind).Must(x => Enum.TryParse<GameActionKind>(x, true, out _)); } }
