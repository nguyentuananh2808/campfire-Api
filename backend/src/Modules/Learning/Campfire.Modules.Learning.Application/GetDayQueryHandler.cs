using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed class GetDayQueryHandler(ILearningService service) : IRequestHandler<GetDayQuery, LearningResult<LearningModels.DayDetail>>
{
    public Task<LearningResult<LearningModels.DayDetail>> Handle(GetDayQuery request, CancellationToken cancellationToken) => service.GetDayAsync(request.UserId, request.DayId, cancellationToken);
}
