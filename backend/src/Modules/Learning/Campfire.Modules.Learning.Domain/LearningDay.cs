namespace Campfire.Modules.Learning.Domain;
public sealed class LearningDay
{
    private LearningDay() { }
    public LearningDay(Guid id, Guid courseId, Guid unitId, int dayNumber, string title, string titleChinese, string description, int estimatedMinutes, int xpReward, bool isReview = false) =>
        (Id, CourseId, UnitId, DayNumber, Title, TitleChinese, Description, EstimatedMinutes, XpReward, IsReview) = (id, courseId, unitId, dayNumber, title, titleChinese, description, estimatedMinutes, xpReward, isReview);
    public Guid Id { get; private set; }
    public Guid CourseId { get; private set; }
    public Guid UnitId { get; private set; }
    public int DayNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string TitleChinese { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int EstimatedMinutes { get; private set; }
    public int XpReward { get; private set; }
    public bool IsReview { get; private set; }
}
