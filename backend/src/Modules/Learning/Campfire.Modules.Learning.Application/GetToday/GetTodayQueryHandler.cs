using MediatR;
using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.GetToday;
public sealed class GetTodayQueryHandler(ILearningService service) : IRequestHandler<GetTodayQuery, LearningResult<LearningModels.Today>>
{
    public Task<LearningResult<LearningModels.Today>> Handle(GetTodayQuery request, CancellationToken cancellationToken) => service.GetTodayAsync(request.UserId, cancellationToken);
}
