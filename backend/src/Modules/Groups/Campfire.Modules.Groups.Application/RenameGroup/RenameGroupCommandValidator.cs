using FluentValidation;

namespace Campfire.Modules.Groups.Application.RenameGroup;

public sealed class RenameGroupCommandValidator : AbstractValidator<RenameGroupCommand>
{
    public RenameGroupCommandValidator()
    {
        RuleFor(command => command.GroupId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MinimumLength(2).MaximumLength(80);
        RuleFor(command => command.TraceId).NotEmpty().MaximumLength(100);
    }
}
