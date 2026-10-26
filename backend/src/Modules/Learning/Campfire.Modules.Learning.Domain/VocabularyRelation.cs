namespace Campfire.Modules.Learning.Domain;
public sealed class VocabularyRelation
{
    private VocabularyRelation() { }
    public VocabularyRelation(Guid sourceId, Guid targetId, string relationType) => (SourceId, TargetId, RelationType) = (sourceId, targetId, relationType);
    public Guid SourceId { get; private set; }
    public Guid TargetId { get; private set; }
    public string RelationType { get; private set; } = string.Empty;
}
