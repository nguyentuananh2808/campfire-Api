using Campfire.Modules.Learning.Domain.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Learning.Infrastructure.Persistence.Configurations;

public sealed class CourseUnitConfiguration : IEntityTypeConfiguration<CourseUnit>
{
    public void Configure(EntityTypeBuilder<CourseUnit> builder)
    {
        builder.ToTable("course_units");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => new { x.CourseId, x.OrderIndex }).IsUnique();
        builder.HasOne<Course>().WithMany().HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
    }
}
