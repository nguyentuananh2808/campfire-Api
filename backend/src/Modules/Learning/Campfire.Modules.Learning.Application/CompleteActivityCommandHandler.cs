using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed class CompleteActivityCommandHandler(ILearningService service) : IRequestHandler<CompleteActivityCommand, LearningResult<LearningModels.Completion>>
{
    public Task<LearningResult<LearningModels.Completion>> Handle(CompleteActivityCommand request, CancellationToken cancellationToken) => service.CompleteActivityAsync(request.UserId, request.ActivityId, request.Score, cancellationToken);
}
