using Campfire.Modules.Events.Domain.Events;
using FluentValidation;

namespace Campfire.Modules.Events.Application.ChangeEventStatus;

public sealed class ChangeEventStatusCommandValidator : AbstractValidator<ChangeEventStatusCommand>
{
    public ChangeEventStatusCommandValidator()
    {
        RuleFor(command => command.ExpectedVersion).NotEmpty();
        RuleFor(command => command.Status).Must(status => Enum.TryParse<EventStatus>(status, true, out var parsed) && parsed != EventStatus.Open)
            .WithMessage("Status must be Confirmed or Cancelled.");
    }
}
