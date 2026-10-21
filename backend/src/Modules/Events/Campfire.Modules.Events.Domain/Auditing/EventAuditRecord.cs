namespace Campfire.Modules.Events.Domain.Auditing;

public sealed class EventAuditRecord
{
    private EventAuditRecord() { }
    public EventAuditRecord(Guid eventId, Guid groupId, Guid actorUserId, string action,
        DateTimeOffset occurredAtUtc, string traceId)
    {
        Id = Guid.NewGuid(); EventId = eventId; GroupId = groupId; ActorUserId = actorUserId;
        Action = action; OccurredAtUtc = occurredAtUtc; TraceId = traceId;
    }
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public string TraceId { get; private set; } = string.Empty;
}
