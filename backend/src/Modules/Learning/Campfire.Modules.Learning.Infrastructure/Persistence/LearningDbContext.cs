using Campfire.Modules.Learning.Contracts;
using Campfire.Modules.Learning.Domain.Courses;
using Campfire.Modules.Learning.Domain.Progress;
using Campfire.Modules.Learning.Domain.Vocabulary;
using Campfire.Modules.Learning.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Learning.Infrastructure.Persistence;

public sealed class LearningDbContext(DbContextOptions<LearningDbContext> options) : DbContext(options)
{
    public const string Schema = LearningModule.Schema;
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseUnit> Units => Set<CourseUnit>();
    public DbSet<LearningDay> Days => Set<LearningDay>();
    public DbSet<LessonActivity> Activities => Set<LessonActivity>();
    public DbSet<UserCourse> UserCourses => Set<UserCourse>();
    public DbSet<UserLearningDayProgress> DayProgress => Set<UserLearningDayProgress>();
    public DbSet<UserActivityProgress> ActivityProgress => Set<UserActivityProgress>();
    public DbSet<Vocabulary> Vocabularies => Set<Vocabulary>();
    public DbSet<VocabularyRelation> VocabularyRelations => Set<VocabularyRelation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LearningDbContext).Assembly);
        LearningSeed.Apply(modelBuilder);
    }
}
