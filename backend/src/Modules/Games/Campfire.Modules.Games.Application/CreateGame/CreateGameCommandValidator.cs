using FluentValidation;

namespace Campfire.Modules.Games.Application.CreateGame;

public sealed class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.OpponentUserId).NotEmpty().NotEqual(x => x.UserId);
    }
}
