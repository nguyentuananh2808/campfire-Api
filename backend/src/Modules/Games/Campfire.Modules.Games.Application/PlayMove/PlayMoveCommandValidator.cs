using FluentValidation;

namespace Campfire.Modules.Games.Application.PlayMove;

public sealed class PlayMoveCommandValidator : AbstractValidator<PlayMoveCommand>
{
    public PlayMoveCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ExpectedVersion).NotEmpty();
        RuleFor(x => x.Cell).InclusiveBetween(0, 8);
    }
}
