namespace Campfire.Modules.Learning.Domain;
public sealed class UserCourse
{
    private UserCourse() { }
    public UserCourse(Guid userId, Guid courseId, DateTimeOffset enrolledAtUtc) => (UserId, CourseId, EnrolledAtUtc) = (userId, courseId, enrolledAtUtc);
    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }
    public int TotalXp { get; private set; }
    public DateTimeOffset EnrolledAtUtc { get; private set; }
    public void AddXp(int xp) => TotalXp += Math.Max(0, xp);
}
