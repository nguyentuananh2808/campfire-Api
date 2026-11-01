using MediatR;
using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.SubmitExercise;
public sealed class SubmitExerciseCommandHandler(ILearningService service) : IRequestHandler<SubmitExerciseCommand, LearningResult<LearningModels.ExerciseResult>>
{
    public Task<LearningResult<LearningModels.ExerciseResult>> Handle(SubmitExerciseCommand request, CancellationToken cancellationToken) => service.SubmitExerciseAsync(request.UserId, request.ActivityId, request.Answers, cancellationToken);
}
