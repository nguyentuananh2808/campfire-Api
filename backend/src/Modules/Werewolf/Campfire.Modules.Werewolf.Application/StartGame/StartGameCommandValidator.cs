using FluentValidation;
namespace Campfire.Modules.Werewolf.Application.StartGame;

public sealed class StartGameCommandValidator : AbstractValidator<StartGameCommand>
{ public StartGameCommandValidator() => RuleFor(x => x.ExpectedVersion).NotEmpty(); }
