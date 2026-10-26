using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed class SubmitExerciseCommandHandler(ILearningService service) : IRequestHandler<SubmitExerciseCommand, LearningResult<LearningModels.ExerciseResult>>
{
    public Task<LearningResult<LearningModels.ExerciseResult>> Handle(SubmitExerciseCommand request, CancellationToken cancellationToken) => service.SubmitExerciseAsync(request.UserId, request.ActivityId, request.Answers, cancellationToken);
}
