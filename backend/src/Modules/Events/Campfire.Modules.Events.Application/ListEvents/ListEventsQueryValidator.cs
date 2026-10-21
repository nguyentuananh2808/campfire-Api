using FluentValidation;

namespace Campfire.Modules.Events.Application.ListEvents;

public sealed class ListEventsQueryValidator : AbstractValidator<ListEventsQuery>
{
    public ListEventsQueryValidator() => RuleFor(query => query.Limit).InclusiveBetween(1, 100);
}
