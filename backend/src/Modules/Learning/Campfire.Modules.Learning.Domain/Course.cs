namespace Campfire.Modules.Learning.Domain;
public sealed class Course
{
    private Course() { }
    public Course(Guid id, string slug, string name, string description, int hskLevel, string contentVersion) => (Id, Slug, Name, Description, HskLevel, ContentVersion) = (id, slug, name, description, hskLevel, contentVersion);
    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int HskLevel { get; private set; }
    public string ContentVersion { get; private set; } = string.Empty;
    public bool IsPublished { get; private set; } = true;
}
