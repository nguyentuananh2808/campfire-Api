using Campfire.Modules.Learning.Domain.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Learning.Infrastructure.Persistence.Configurations;

public sealed class LearningDayConfiguration : IEntityTypeConfiguration<LearningDay>
{
    public void Configure(EntityTypeBuilder<LearningDay> builder)
    {
        builder.ToTable("learning_days", table =>
        {
            table.HasCheckConstraint("CK_learning_days_DayNumber", "\"DayNumber\" > 0");
            table.HasCheckConstraint("CK_learning_days_XpReward", "\"XpReward\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(120).IsRequired();
        builder.Property(x => x.TitleChinese).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => new { x.CourseId, x.DayNumber }).IsUnique();
        builder.HasOne<Course>().WithMany().HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CourseUnit>().WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
    }
}
