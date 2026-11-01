using MediatR;
using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.GetMindMap;
public sealed class GetMindMapQueryHandler(ILearningService service) : IRequestHandler<GetMindMapQuery, LearningResult<LearningModels.MindMap>>
{
    public Task<LearningResult<LearningModels.MindMap>> Handle(GetMindMapQuery request, CancellationToken cancellationToken) => service.GetMindMapAsync(request.UserId, request.Word, request.HskLevel, request.MaxChildren, cancellationToken);
}
