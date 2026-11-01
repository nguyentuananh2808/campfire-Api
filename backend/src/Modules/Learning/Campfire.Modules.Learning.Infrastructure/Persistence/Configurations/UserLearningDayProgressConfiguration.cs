using Campfire.Modules.Learning.Domain.Courses;
using Campfire.Modules.Learning.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Learning.Infrastructure.Persistence.Configurations;

public sealed class UserLearningDayProgressConfiguration : IEntityTypeConfiguration<UserLearningDayProgress>
{
    public void Configure(EntityTypeBuilder<UserLearningDayProgress> builder)
    {
        builder.ToTable("user_day_progress", table =>
        {
            table.HasCheckConstraint("CK_user_day_progress_Status", "\"Status\" IN ('Available','InProgress','Completed')");
            table.HasCheckConstraint("CK_user_day_progress_Percent", "\"ProgressPercent\" BETWEEN 0 AND 100");
        });
        builder.HasKey(x => new { x.UserId, x.LearningDayId });
        builder.Property(x => x.Status).HasMaxLength(16).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.Status });
        builder.HasOne<LearningDay>().WithMany().HasForeignKey(x => x.LearningDayId).OnDelete(DeleteBehavior.Cascade);
    }
}
