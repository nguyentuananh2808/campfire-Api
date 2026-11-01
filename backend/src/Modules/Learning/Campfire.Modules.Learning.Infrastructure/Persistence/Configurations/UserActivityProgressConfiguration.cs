using Campfire.Modules.Learning.Domain.Courses;
using Campfire.Modules.Learning.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Learning.Infrastructure.Persistence.Configurations;

public sealed class UserActivityProgressConfiguration : IEntityTypeConfiguration<UserActivityProgress>
{
    public void Configure(EntityTypeBuilder<UserActivityProgress> builder)
    {
        builder.ToTable("user_activity_progress", table => table.HasCheckConstraint("CK_user_activity_progress_Score", "\"Score\" BETWEEN 0 AND 100"));
        builder.HasKey(x => new { x.UserId, x.ActivityId });
        builder.HasOne<LessonActivity>().WithMany().HasForeignKey(x => x.ActivityId).OnDelete(DeleteBehavior.Cascade);
    }
}
