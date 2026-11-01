using Campfire.Modules.Learning.Domain.Courses;
using Campfire.Modules.Learning.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Learning.Infrastructure.Persistence.Configurations;

public sealed class UserCourseConfiguration : IEntityTypeConfiguration<UserCourse>
{
    public void Configure(EntityTypeBuilder<UserCourse> builder)
    {
        builder.ToTable("user_courses", table => table.HasCheckConstraint("CK_user_courses_TotalXp", "\"TotalXp\" >= 0"));
        builder.HasKey(x => new { x.UserId, x.CourseId });
        builder.HasOne<Course>().WithMany().HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
    }
}
