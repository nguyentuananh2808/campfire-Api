using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed record SubmitExerciseCommand(Guid UserId, Guid ActivityId, IReadOnlyList<LearningModels.ExerciseAnswer> Answers) : IRequest<LearningResult<LearningModels.ExerciseResult>>;
