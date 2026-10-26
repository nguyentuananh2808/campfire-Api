using Campfire.Modules.Learning.Domain;

namespace Campfire.UnitTests;

public sealed class LearningProgressTests
{
    [Fact]
    public void Completing_a_day_awards_xp_only_once()
    {
        var progress = new UserLearningDayProgress(Guid.NewGuid(), Guid.NewGuid(), "Available", DateTimeOffset.UtcNow);
        progress.UpdateProgress(4, 4);

        Assert.True(progress.Complete(100, DateTimeOffset.UtcNow));
        Assert.False(progress.Complete(100, DateTimeOffset.UtcNow.AddMinutes(1)));
        Assert.Equal("Completed", progress.Status);
        Assert.Equal(100, progress.XpEarned);
    }

    [Fact]
    public void Activity_score_is_bounded_to_database_range()
    {
        var high = new UserActivityProgress(Guid.NewGuid(), Guid.NewGuid(), 120, DateTimeOffset.UtcNow);
        var low = new UserActivityProgress(Guid.NewGuid(), Guid.NewGuid(), -5, DateTimeOffset.UtcNow);

        Assert.Equal(100, high.Score);
        Assert.Equal(0, low.Score);
    }
}
