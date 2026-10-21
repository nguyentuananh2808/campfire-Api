namespace Campfire.Modules.Events.Domain.Events;

public sealed class EventRsvp
{
    private EventRsvp() { }
    public EventRsvp(Guid eventId, Guid userId, RsvpResponse response, DateTimeOffset respondedAtUtc)
    {
        EventId = eventId;
        UserId = userId;
        Response = response;
        RespondedAtUtc = respondedAtUtc;
    }

    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public RsvpResponse Response { get; private set; }
    public DateTimeOffset RespondedAtUtc { get; private set; }
}
