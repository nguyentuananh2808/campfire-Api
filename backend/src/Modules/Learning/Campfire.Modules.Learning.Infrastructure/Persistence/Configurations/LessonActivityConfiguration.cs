using Campfire.Modules.Learning.Domain.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Learning.Infrastructure.Persistence.Configurations;

public sealed class LessonActivityConfiguration : IEntityTypeConfiguration<LessonActivity>
{
    public void Configure(EntityTypeBuilder<LessonActivity> builder)
    {
        builder.ToTable("lesson_activities", table => table.HasCheckConstraint("CK_lesson_activities_Type", "\"Type\" IN ('VOCABULARY','SENTENCE','GRAMMAR','EXERCISE')"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(24).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ContentJson).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => new { x.LearningDayId, x.OrderIndex }).IsUnique();
        builder.HasOne<LearningDay>().WithMany().HasForeignKey(x => x.LearningDayId).OnDelete(DeleteBehavior.Cascade);
    }
}
