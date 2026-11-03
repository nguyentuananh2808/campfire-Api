using Campfire.Modules.Learning.Domain.Reading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Learning.Infrastructure.Persistence.Configurations;

public sealed class ReadingDocumentConfiguration : IEntityTypeConfiguration<ReadingDocument>
{
    public void Configure(EntityTypeBuilder<ReadingDocument> builder)
    {
        builder.ToTable("reading_documents", table =>
        {
            table.HasCheckConstraint("CK_reading_documents_CharacterCount", "\"CharacterCount\" BETWEEN 1 AND 10000");
            table.HasCheckConstraint("CK_reading_documents_ChineseCharacterCount", "\"ChineseCharacterCount\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(10_000).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
    }
}
