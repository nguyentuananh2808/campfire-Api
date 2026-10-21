using FluentValidation;

namespace Campfire.Modules.Events.Application.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(command => command.GroupId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().Must(value => !string.IsNullOrWhiteSpace(value)).MaximumLength(120);
        RuleFor(command => command.Description).NotNull().MaximumLength(2000);
        RuleFor(command => command.StartsAtUtc).NotEmpty();
        RuleFor(command => command.Location).NotEmpty().Must(value => !string.IsNullOrWhiteSpace(value)).MaximumLength(200);
    }
}
