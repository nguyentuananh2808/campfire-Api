using MediatR;
using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.GetDay;
public sealed class GetDayQueryHandler(ILearningService service) : IRequestHandler<GetDayQuery, LearningResult<LearningModels.DayDetail>>
{
    public Task<LearningResult<LearningModels.DayDetail>> Handle(GetDayQuery request, CancellationToken cancellationToken) => service.GetDayAsync(request.UserId, request.DayId, cancellationToken);
}
