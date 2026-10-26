namespace Campfire.Modules.Learning.Domain;
public sealed class UserActivityProgress
{
    private UserActivityProgress() { }
    public UserActivityProgress(Guid userId, Guid activityId, int score, DateTimeOffset completedAtUtc) => (UserId, ActivityId, Score, CompletedAtUtc) = (userId, activityId, Math.Clamp(score, 0, 100), completedAtUtc);
    public Guid UserId { get; private set; }
    public Guid ActivityId { get; private set; }
    public int Score { get; private set; }
    public DateTimeOffset CompletedAtUtc { get; private set; }
}
