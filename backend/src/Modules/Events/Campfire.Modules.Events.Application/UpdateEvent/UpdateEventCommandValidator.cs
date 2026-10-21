using FluentValidation;

namespace Campfire.Modules.Events.Application.UpdateEvent;

public sealed class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(command => command.ExpectedVersion).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().Must(value => !string.IsNullOrWhiteSpace(value)).MaximumLength(120);
        RuleFor(command => command.Description).NotNull().MaximumLength(2000);
        RuleFor(command => command.StartsAtUtc).NotEmpty();
        RuleFor(command => command.Location).NotEmpty().Must(value => !string.IsNullOrWhiteSpace(value)).MaximumLength(200);
    }
}
