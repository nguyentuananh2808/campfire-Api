using System.Data;
using Campfire.Modules.Learning.Application;
using Campfire.Modules.Learning.Domain;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Learning.Infrastructure;

public sealed class EfLearningService(LearningDbContext dbContext, TimeProvider timeProvider) : ILearningService
{
    public async Task<LearningResult<LearningModels.Today>> GetTodayAsync(Guid userId, CancellationToken cancellationToken)
    {
        var path = await BuildPathAsync(userId, "hsk1", cancellationToken);
        if (!path.IsSuccess) return LearningResult<LearningModels.Today>.Failure(path.ErrorCode!, path.Error!);
        var day = path.Value!.Units.SelectMany(x => x.Days).FirstOrDefault(x => x.Status is "Available" or "InProgress") ?? path.Value.Units.SelectMany(x => x.Days).Last();
        var detail = await GetDayAsync(userId, day.Id, cancellationToken);
        return detail.IsSuccess ? LearningResult<LearningModels.Today>.Success(new(path.Value.Course, path.Value.TotalXp, detail.Value!)) : LearningResult<LearningModels.Today>.Failure(detail.ErrorCode!, detail.Error!);
    }

    public Task<LearningResult<LearningModels.CoursePath>> GetPathAsync(Guid userId, string slug, CancellationToken cancellationToken) => BuildPathAsync(userId, slug, cancellationToken);

    public async Task<LearningResult<LearningModels.DayDetail>> GetDayAsync(Guid userId, Guid dayId, CancellationToken cancellationToken)
    {
        var day = await dbContext.Days.AsNoTracking().SingleOrDefaultAsync(x => x.Id == dayId, cancellationToken);
        if (day is null) return LearningResult<LearningModels.DayDetail>.Failure("not_found", "Không tìm thấy bài học.");
        var course = await dbContext.Courses.AsNoTracking().SingleAsync(x => x.Id == day.CourseId, cancellationToken);
        var path = await BuildPathAsync(userId, course.Slug, cancellationToken);
        var summary = path.Value!.Units.SelectMany(x => x.Days).Single(x => x.Id == dayId);
        if (summary.Status == "Locked") return LearningResult<LearningModels.DayDetail>.Failure("forbidden", "Hãy hoàn thành bài trước để mở khóa ngày này.");
        var completed = await dbContext.ActivityProgress.AsNoTracking().Where(x => x.UserId == userId).Select(x => x.ActivityId).ToHashSetAsync(cancellationToken);
        var activityEntities = await dbContext.Activities.AsNoTracking()
            .Where(x => x.LearningDayId == dayId)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
        var activities = activityEntities
            .Select(x => new LearningModels.ActivitySummary(
                x.Id,
                x.Type,
                x.Title,
                x.OrderIndex,
                x.Required,
                completed.Contains(x.Id),
                x.Type == "EXERCISE" ? ExerciseScorer.WithoutAnswers(x.ContentJson) : x.ContentJson))
            .ToList();
        return LearningResult<LearningModels.DayDetail>.Success(new(ToCourse(course), summary, activities));
    }

    public async Task<LearningResult<LearningModels.Completion>> CompleteActivityAsync(Guid userId, Guid activityId, int score, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var activity = await dbContext.Activities.SingleOrDefaultAsync(x => x.Id == activityId, cancellationToken);
        if (activity is null) return LearningResult<LearningModels.Completion>.Failure("not_found", "Không tìm thấy hoạt động.");
        if (activity.Type == "EXERCISE") return LearningResult<LearningModels.Completion>.Failure("validation_error", "Bài tập phải được nộp để chấm điểm.");
        var access = await CanAccessDayAsync(userId, activity.LearningDayId, cancellationToken);
        if (!access) return LearningResult<LearningModels.Completion>.Failure("forbidden", "Ngày học chưa được mở khóa.");
        if (!await dbContext.ActivityProgress.AnyAsync(x => x.UserId == userId && x.ActivityId == activityId, cancellationToken)) dbContext.ActivityProgress.Add(new(userId, activityId, score, timeProvider.GetUtcNow()));
        var result = await FinalizeDayAsync(userId, activity.LearningDayId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<LearningResult<LearningModels.ExerciseResult>> SubmitExerciseAsync(Guid userId, Guid activityId, IReadOnlyList<LearningModels.ExerciseAnswer> answers, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var activity = await dbContext.Activities.SingleOrDefaultAsync(x => x.Id == activityId && x.Type == "EXERCISE", cancellationToken);
        if (activity is null) return LearningResult<LearningModels.ExerciseResult>.Failure("not_found", "Không tìm thấy bài tập.");
        if (!await CanAccessDayAsync(userId, activity.LearningDayId, cancellationToken)) return LearningResult<LearningModels.ExerciseResult>.Failure("forbidden", "Ngày học chưa được mở khóa.");
        var score = ExerciseScorer.Score(activity.ContentJson, answers);
        if (!await dbContext.ActivityProgress.AnyAsync(x => x.UserId == userId && x.ActivityId == activityId, cancellationToken)) dbContext.ActivityProgress.Add(new(userId, activityId, score.Score, timeProvider.GetUtcNow()));
        var completion = await FinalizeDayAsync(userId, activity.LearningDayId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return completion.IsSuccess ? LearningResult<LearningModels.ExerciseResult>.Success(new(score.Score, score.Correct, score.Total, completion.Value!)) : LearningResult<LearningModels.ExerciseResult>.Failure(completion.ErrorCode!, completion.Error!);
    }

    public async Task<LearningResult<LearningModels.MindMap>> GetMindMapAsync(Guid userId, string word, int hskLevel, int maxChildren, CancellationToken cancellationToken)
    {
        _ = userId;
        var root = await dbContext.Vocabularies.AsNoTracking().SingleOrDefaultAsync(x => x.Chinese == word.Trim() && x.HskLevel <= hskLevel, cancellationToken);
        if (root is null) return LearningResult<LearningModels.MindMap>.Failure("not_found", "Từ này chưa có trong curriculum đã kiểm duyệt. AI expansion chưa được bật vì chưa cấu hình provider.");
        var relations = await dbContext.VocabularyRelations.AsNoTracking().Where(x => x.SourceId == root.Id).Take(maxChildren).ToListAsync(cancellationToken);
        var ids = relations.Select(x => x.TargetId).ToArray();
        var targets = await dbContext.Vocabularies.AsNoTracking().Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        var children = relations.Where(x => targets.ContainsKey(x.TargetId)).Select(x => ToNode(targets[x.TargetId], x.RelationType)).ToList();
        return LearningResult<LearningModels.MindMap>.Success(new(ToNode(root, "Root"), children, "CuratedCurriculum", false));
    }

    public LearningModels.Capabilities GetCapabilities() => new(false, false, false, false, "TTS, chấm phát âm và AI expansion sẽ được bật khi có provider thật; bản base không mô phỏng kết quả AI.");

    private async Task<LearningResult<LearningModels.CoursePath>> BuildPathAsync(Guid userId, string slug, CancellationToken cancellationToken)
    {
        var course = await dbContext.Courses.AsNoTracking().SingleOrDefaultAsync(x => x.Slug == slug && x.IsPublished, cancellationToken);
        if (course is null) return LearningResult<LearningModels.CoursePath>.Failure("not_found", "Không tìm thấy khóa học.");
        var units = await dbContext.Units.AsNoTracking().Where(x => x.CourseId == course.Id).OrderBy(x => x.OrderIndex).ToListAsync(cancellationToken);
        var days = await dbContext.Days.AsNoTracking().Where(x => x.CourseId == course.Id).OrderBy(x => x.DayNumber).ToListAsync(cancellationToken);
        var progress = await dbContext.DayProgress.AsNoTracking().Where(x => x.UserId == userId).ToDictionaryAsync(x => x.LearningDayId, cancellationToken);
        var totalXp = await dbContext.UserCourses.AsNoTracking().Where(x => x.UserId == userId && x.CourseId == course.Id).Select(x => (int?)x.TotalXp).SingleOrDefaultAsync(cancellationToken) ?? 0;
        var previousComplete = true;
        var summaries = new List<LearningModels.DaySummary>();
        foreach (var day in days)
        {
            progress.TryGetValue(day.Id, out var current);
            var status = current?.Status ?? (previousComplete ? "Available" : "Locked");
            summaries.Add(new(day.Id, day.DayNumber, day.Title, day.TitleChinese, day.Description, day.EstimatedMinutes, day.XpReward, status, current?.ProgressPercent ?? 0));
            previousComplete = status == "Completed";
        }
        return LearningResult<LearningModels.CoursePath>.Success(new(ToCourse(course), totalXp, units.Select(u => new LearningModels.UnitPath(u.Id, u.Title, u.OrderIndex, summaries.Where(d => days.Single(x => x.Id == d.Id).UnitId == u.Id).ToList())).ToList()));
    }

    private async Task<bool> CanAccessDayAsync(Guid userId, Guid dayId, CancellationToken cancellationToken)
    {
        var day = await dbContext.Days.AsNoTracking().SingleAsync(x => x.Id == dayId, cancellationToken);
        if (day.DayNumber == 1) return true;
        var previousId = await dbContext.Days.AsNoTracking().Where(x => x.CourseId == day.CourseId && x.DayNumber == day.DayNumber - 1).Select(x => x.Id).SingleAsync(cancellationToken);
        return await dbContext.DayProgress.AnyAsync(x => x.UserId == userId && x.LearningDayId == previousId && x.Status == "Completed", cancellationToken);
    }

    private async Task<LearningResult<LearningModels.Completion>> FinalizeDayAsync(Guid userId, Guid dayId, CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        var required = await dbContext.Activities.Where(x => x.LearningDayId == dayId && x.Required).Select(x => x.Id).ToListAsync(cancellationToken);
        var completed = await dbContext.ActivityProgress.CountAsync(x => x.UserId == userId && required.Contains(x.ActivityId), cancellationToken);
        var progress = await dbContext.DayProgress.SingleOrDefaultAsync(x => x.UserId == userId && x.LearningDayId == dayId, cancellationToken) ?? new UserLearningDayProgress(userId, dayId, "Available", timeProvider.GetUtcNow());
        if (dbContext.Entry(progress).State == EntityState.Detached) dbContext.DayProgress.Add(progress);
        progress.UpdateProgress(completed, required.Count);
        var day = await dbContext.Days.SingleAsync(x => x.Id == dayId, cancellationToken);
        var awarded = 0;
        if (completed == required.Count && progress.Complete(day.XpReward, timeProvider.GetUtcNow()))
        {
            awarded = day.XpReward;
            var course = await dbContext.UserCourses.SingleOrDefaultAsync(x => x.UserId == userId && x.CourseId == day.CourseId, cancellationToken) ?? new UserCourse(userId, day.CourseId, timeProvider.GetUtcNow());
            if (dbContext.Entry(course).State == EntityState.Detached) dbContext.UserCourses.Add(course);
            course.AddXp(awarded);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        var detail = await GetDayAsync(userId, dayId, cancellationToken);
        var total = await dbContext.UserCourses.Where(x => x.UserId == userId && x.CourseId == day.CourseId).Select(x => (int?)x.TotalXp).SingleOrDefaultAsync(cancellationToken) ?? 0;
        return LearningResult<LearningModels.Completion>.Success(new(detail.Value!, total, awarded, progress.Status == "Completed"));
    }

    private static LearningModels.CourseSummary ToCourse(Course x) => new(x.Id, x.Slug, x.Name, x.Description, x.HskLevel, x.ContentVersion);
    private static LearningModels.MindMapNode ToNode(Vocabulary x, string relation) => new(x.Id, x.Chinese, x.Pinyin, x.MeaningVi, x.HskLevel, x.ExampleChinese, x.ExamplePinyin, x.ExampleTranslationVi, relation, 0);
}
