using FluentValidation;

namespace Campfire.Modules.Groups.Application.CreateGroup;

public sealed class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(80);
        RuleFor(command => command.TraceId).NotEmpty().MaximumLength(100);
    }
}
