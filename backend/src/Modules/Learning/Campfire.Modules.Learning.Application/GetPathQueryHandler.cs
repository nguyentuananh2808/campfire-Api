using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed class GetPathQueryHandler(ILearningService service) : IRequestHandler<GetPathQuery, LearningResult<LearningModels.CoursePath>>
{
    public Task<LearningResult<LearningModels.CoursePath>> Handle(GetPathQuery request, CancellationToken cancellationToken) => service.GetPathAsync(request.UserId, request.Slug, cancellationToken);
}
