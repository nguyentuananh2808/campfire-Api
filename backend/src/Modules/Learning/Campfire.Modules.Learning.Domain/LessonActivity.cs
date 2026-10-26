namespace Campfire.Modules.Learning.Domain;
public sealed class LessonActivity
{
    private LessonActivity() { }
    public LessonActivity(Guid id, Guid learningDayId, string type, string title, int orderIndex, bool required, string contentJson) =>
        (Id, LearningDayId, Type, Title, OrderIndex, Required, ContentJson) = (id, learningDayId, type, title, orderIndex, required, contentJson);
    public Guid Id { get; private set; }
    public Guid LearningDayId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public int OrderIndex { get; private set; }
    public bool Required { get; private set; }
    public string ContentJson { get; private set; } = "{}";
}
