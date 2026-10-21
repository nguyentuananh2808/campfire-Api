using FluentValidation;

namespace Campfire.Modules.Groups.Application.JoinGroup;

public sealed class JoinGroupCommandValidator : AbstractValidator<JoinGroupCommand>
{
    public JoinGroupCommandValidator()
    {
        RuleFor(command => command.Token).NotEmpty().MaximumLength(128);
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.TraceId).NotEmpty().MaximumLength(100);
    }
}
