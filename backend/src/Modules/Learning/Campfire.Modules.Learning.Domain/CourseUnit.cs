namespace Campfire.Modules.Learning.Domain;
public sealed class CourseUnit
{
    private CourseUnit() { }
    public CourseUnit(Guid id, Guid courseId, int orderIndex, string title) => (Id, CourseId, OrderIndex, Title) = (id, courseId, orderIndex, title);
    public Guid Id { get; private set; }
    public Guid CourseId { get; private set; }
    public int OrderIndex { get; private set; }
    public string Title { get; private set; } = string.Empty;
}
