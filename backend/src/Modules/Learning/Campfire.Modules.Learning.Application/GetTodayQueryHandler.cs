using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed class GetTodayQueryHandler(ILearningService service) : IRequestHandler<GetTodayQuery, LearningResult<LearningModels.Today>>
{
    public Task<LearningResult<LearningModels.Today>> Handle(GetTodayQuery request, CancellationToken cancellationToken) => service.GetTodayAsync(request.UserId, cancellationToken);
}
