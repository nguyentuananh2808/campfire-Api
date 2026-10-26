namespace Campfire.Modules.Learning.Domain;
public sealed class UserLearningDayProgress
{
    private UserLearningDayProgress() { }
    public UserLearningDayProgress(Guid userId, Guid learningDayId, string status, DateTimeOffset startedAtUtc) => (UserId, LearningDayId, Status, StartedAtUtc) = (userId, learningDayId, status, startedAtUtc);
    public Guid UserId { get; private set; }
    public Guid LearningDayId { get; private set; }
    public string Status { get; private set; } = "Available";
    public int ProgressPercent { get; private set; }
    public int XpEarned { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public void UpdateProgress(int completed, int total) { ProgressPercent = total == 0 ? 0 : completed * 100 / total; if (Status == "Available") Status = "InProgress"; }
    public bool Complete(int xp, DateTimeOffset now) { if (Status == "Completed") return false; Status = "Completed"; ProgressPercent = 100; XpEarned = xp; CompletedAtUtc = now; return true; }
}
