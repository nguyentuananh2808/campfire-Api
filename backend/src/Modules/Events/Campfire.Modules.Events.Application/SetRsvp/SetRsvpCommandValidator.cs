using Campfire.Modules.Events.Domain.Events;
using FluentValidation;

namespace Campfire.Modules.Events.Application.SetRsvp;

public sealed class SetRsvpCommandValidator : AbstractValidator<SetRsvpCommand>
{
    public SetRsvpCommandValidator() =>
        RuleFor(command => command.Response).Must(response => Enum.TryParse<RsvpResponse>(response, true, out _))
            .WithMessage("Response must be Going or NotGoing.");
}
