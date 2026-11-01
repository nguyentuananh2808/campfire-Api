using Campfire.Modules.Learning.Domain.Vocabulary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Learning.Infrastructure.Persistence.Configurations;

public sealed class VocabularyConfiguration : IEntityTypeConfiguration<Vocabulary>
{
    public void Configure(EntityTypeBuilder<Vocabulary> builder)
    {
        builder.ToTable("vocabularies", table => table.HasCheckConstraint("CK_vocabularies_HskLevel", "\"HskLevel\" BETWEEN 1 AND 6"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Chinese).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Pinyin).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MeaningVi).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ExampleChinese).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ExamplePinyin).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ExampleTranslationVi).HasMaxLength(300).IsRequired();
        builder.HasIndex(x => new { x.Chinese, x.HskLevel }).IsUnique();
    }
}
