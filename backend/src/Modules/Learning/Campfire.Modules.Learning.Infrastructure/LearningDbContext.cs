using Campfire.Modules.Learning.Contracts;
using Campfire.Modules.Learning.Domain;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Learning.Infrastructure;

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
        ConfigureCourse(modelBuilder.Entity<Course>());
        ConfigureUnit(modelBuilder.Entity<CourseUnit>());
        ConfigureDay(modelBuilder.Entity<LearningDay>());
        ConfigureActivity(modelBuilder.Entity<LessonActivity>());
        ConfigureProgress(modelBuilder);
        ConfigureVocabulary(modelBuilder);
        LearningSeed.Apply(modelBuilder);
    }

    private static void ConfigureCourse(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Course> b)
    {
        b.ToTable("courses"); b.HasKey(x => x.Id); b.Property(x => x.Slug).HasMaxLength(40).IsRequired(); b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Name).HasMaxLength(120).IsRequired(); b.Property(x => x.Description).HasMaxLength(500).IsRequired(); b.Property(x => x.ContentVersion).HasMaxLength(20).IsRequired();
    }
    private static void ConfigureUnit(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<CourseUnit> b)
    {
        b.ToTable("course_units"); b.HasKey(x => x.Id); b.Property(x => x.Title).HasMaxLength(120).IsRequired(); b.HasIndex(x => new { x.CourseId, x.OrderIndex }).IsUnique(); b.HasOne<Course>().WithMany().HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
    }
    private static void ConfigureDay(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<LearningDay> b)
    {
        b.ToTable("learning_days", t => { t.HasCheckConstraint("CK_learning_days_DayNumber", "\"DayNumber\" > 0"); t.HasCheckConstraint("CK_learning_days_XpReward", "\"XpReward\" >= 0"); }); b.HasKey(x => x.Id); b.Property(x => x.Title).HasMaxLength(120).IsRequired(); b.Property(x => x.TitleChinese).HasMaxLength(80).IsRequired(); b.Property(x => x.Description).HasMaxLength(500).IsRequired(); b.HasIndex(x => new { x.CourseId, x.DayNumber }).IsUnique(); b.HasOne<Course>().WithMany().HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade); b.HasOne<CourseUnit>().WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
    }
    private static void ConfigureActivity(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<LessonActivity> b)
    {
        b.ToTable("lesson_activities", t => t.HasCheckConstraint("CK_lesson_activities_Type", "\"Type\" IN ('VOCABULARY','SENTENCE','GRAMMAR','EXERCISE')")); b.HasKey(x => x.Id); b.Property(x => x.Type).HasMaxLength(24).IsRequired(); b.Property(x => x.Title).HasMaxLength(120).IsRequired(); b.Property(x => x.ContentJson).HasColumnType("jsonb").IsRequired(); b.HasIndex(x => new { x.LearningDayId, x.OrderIndex }).IsUnique(); b.HasOne<LearningDay>().WithMany().HasForeignKey(x => x.LearningDayId).OnDelete(DeleteBehavior.Cascade);
    }
    private static void ConfigureProgress(ModelBuilder m)
    {
        var uc = m.Entity<UserCourse>(); uc.ToTable("user_courses", t => t.HasCheckConstraint("CK_user_courses_TotalXp", "\"TotalXp\" >= 0")); uc.HasKey(x => new { x.UserId, x.CourseId }); uc.HasOne<Course>().WithMany().HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
        var dp = m.Entity<UserLearningDayProgress>(); dp.ToTable("user_day_progress", t => { t.HasCheckConstraint("CK_user_day_progress_Status", "\"Status\" IN ('Available','InProgress','Completed')"); t.HasCheckConstraint("CK_user_day_progress_Percent", "\"ProgressPercent\" BETWEEN 0 AND 100"); }); dp.HasKey(x => new { x.UserId, x.LearningDayId }); dp.Property(x => x.Status).HasMaxLength(16).IsRequired(); dp.HasIndex(x => new { x.UserId, x.Status }); dp.HasOne<LearningDay>().WithMany().HasForeignKey(x => x.LearningDayId).OnDelete(DeleteBehavior.Cascade);
        var ap = m.Entity<UserActivityProgress>(); ap.ToTable("user_activity_progress", t => t.HasCheckConstraint("CK_user_activity_progress_Score", "\"Score\" BETWEEN 0 AND 100")); ap.HasKey(x => new { x.UserId, x.ActivityId }); ap.HasOne<LessonActivity>().WithMany().HasForeignKey(x => x.ActivityId).OnDelete(DeleteBehavior.Cascade);
    }
    private static void ConfigureVocabulary(ModelBuilder m)
    {
        var v = m.Entity<Vocabulary>(); v.ToTable("vocabularies", t => t.HasCheckConstraint("CK_vocabularies_HskLevel", "\"HskLevel\" BETWEEN 1 AND 6")); v.HasKey(x => x.Id); v.Property(x => x.Chinese).HasMaxLength(40).IsRequired(); v.Property(x => x.Pinyin).HasMaxLength(100).IsRequired(); v.Property(x => x.MeaningVi).HasMaxLength(200).IsRequired(); v.Property(x => x.ExampleChinese).HasMaxLength(200).IsRequired(); v.Property(x => x.ExamplePinyin).HasMaxLength(300).IsRequired(); v.Property(x => x.ExampleTranslationVi).HasMaxLength(300).IsRequired(); v.HasIndex(x => new { x.Chinese, x.HskLevel }).IsUnique();
        var r = m.Entity<VocabularyRelation>(); r.ToTable("vocabulary_relations", t => t.HasCheckConstraint("CK_vocabulary_relations_Type", "\"RelationType\" IN ('Compound','Semantic')")); r.HasKey(x => new { x.SourceId, x.TargetId, x.RelationType }); r.Property(x => x.RelationType).HasMaxLength(24).IsRequired(); r.HasIndex(x => x.SourceId); r.HasOne<Vocabulary>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Cascade); r.HasOne<Vocabulary>().WithMany().HasForeignKey(x => x.TargetId).OnDelete(DeleteBehavior.Restrict);
    }
}
