namespace Campfire.Modules.Groups.Domain.Auditing;

public sealed class GroupAuditRecord
{
    private GroupAuditRecord()
    {
    }

    public GroupAuditRecord(
        Guid groupId,
        Guid actorUserId,
        string action,
        Guid? subjectUserId,
        DateTimeOffset occurredAtUtc,
        string traceId)
    {
        Id = Guid.NewGuid();
        GroupId = groupId;
        ActorUserId = actorUserId;
        Action = action;
        SubjectUserId = subjectUserId;
        OccurredAtUtc = occurredAtUtc;
        TraceId = traceId;
    }

    public Guid Id { get; private set; }

    public Guid GroupId { get; private set; }

    public Guid ActorUserId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public Guid? SubjectUserId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string TraceId { get; private set; } = string.Empty;
}
