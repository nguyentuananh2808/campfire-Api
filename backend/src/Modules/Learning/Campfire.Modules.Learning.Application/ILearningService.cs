namespace Campfire.Modules.Learning.Application;

public interface ILearningService
{
    Task<LearningResult<LearningModels.Today>> GetTodayAsync(Guid userId, CancellationToken cancellationToken);
    Task<LearningResult<LearningModels.CoursePath>> GetPathAsync(Guid userId, string slug, CancellationToken cancellationToken);
    Task<LearningResult<LearningModels.DayDetail>> GetDayAsync(Guid userId, Guid dayId, CancellationToken cancellationToken);
    Task<LearningResult<LearningModels.Completion>> CompleteActivityAsync(Guid userId, Guid activityId, int score, CancellationToken cancellationToken);
    Task<LearningResult<LearningModels.ExerciseResult>> SubmitExerciseAsync(Guid userId, Guid activityId, IReadOnlyList<LearningModels.ExerciseAnswer> answers, CancellationToken cancellationToken);
    Task<LearningResult<LearningModels.MindMap>> GetMindMapAsync(Guid userId, string word, int hskLevel, int maxChildren, CancellationToken cancellationToken);
    LearningModels.Capabilities GetCapabilities();
}
