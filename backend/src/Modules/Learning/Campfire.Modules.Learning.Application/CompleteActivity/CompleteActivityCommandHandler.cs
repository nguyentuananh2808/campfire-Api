using MediatR;
using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.CompleteActivity;
public sealed class CompleteActivityCommandHandler(ILearningService service) : IRequestHandler<CompleteActivityCommand, LearningResult<LearningModels.Completion>>
{
    public Task<LearningResult<LearningModels.Completion>> Handle(CompleteActivityCommand request, CancellationToken cancellationToken) => service.CompleteActivityAsync(request.UserId, request.ActivityId, request.Score, cancellationToken);
}
