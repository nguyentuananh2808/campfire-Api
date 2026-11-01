using MediatR;
using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.GetPath;
public sealed class GetPathQueryHandler(ILearningService service) : IRequestHandler<GetPathQuery, LearningResult<LearningModels.CoursePath>>
{
    public Task<LearningResult<LearningModels.CoursePath>> Handle(GetPathQuery request, CancellationToken cancellationToken) => service.GetPathAsync(request.UserId, request.Slug, cancellationToken);
}
