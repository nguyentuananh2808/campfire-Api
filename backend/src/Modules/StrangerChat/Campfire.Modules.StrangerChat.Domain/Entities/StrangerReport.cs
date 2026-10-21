namespace Campfire.Modules.StrangerChat.Domain.Entities;

public sealed class StrangerReport
{
    private StrangerReport() { }
    public StrangerReport(Guid conversationId, Guid reporterSessionId, Guid reportedSessionId, string reason, string? note, DateTimeOffset now)
    { Id = Guid.NewGuid(); ConversationId = conversationId; ReporterSessionId = reporterSessionId; ReportedSessionId = reportedSessionId; Reason = reason; Note = note; CreatedAtUtc = now; RetainUntilUtc = now.AddDays(90); }
    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid ReporterSessionId { get; private set; }
    public Guid ReportedSessionId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Note { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset RetainUntilUtc { get; private set; }
}
