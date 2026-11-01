using MediatR;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.SubmitExercise;
public sealed record SubmitExerciseCommand(Guid UserId, Guid ActivityId, IReadOnlyList<LearningModels.ExerciseAnswer> Answers) : IRequest<LearningResult<LearningModels.ExerciseResult>>;
