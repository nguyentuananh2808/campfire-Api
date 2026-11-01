using Campfire.Modules.Learning.Domain.Vocabulary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Learning.Infrastructure.Persistence.Configurations;

public sealed class VocabularyRelationConfiguration : IEntityTypeConfiguration<VocabularyRelation>
{
    public void Configure(EntityTypeBuilder<VocabularyRelation> builder)
    {
        builder.ToTable("vocabulary_relations", table => table.HasCheckConstraint("CK_vocabulary_relations_Type", "\"RelationType\" IN ('Compound','Semantic')"));
        builder.HasKey(x => new { x.SourceId, x.TargetId, x.RelationType });
        builder.Property(x => x.RelationType).HasMaxLength(24).IsRequired();
        builder.HasIndex(x => x.SourceId);
        builder.HasOne<Vocabulary>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Vocabulary>().WithMany().HasForeignKey(x => x.TargetId).OnDelete(DeleteBehavior.Restrict);
    }
}
