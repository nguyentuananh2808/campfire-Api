using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed class GetMindMapQueryHandler(ILearningService service) : IRequestHandler<GetMindMapQuery, LearningResult<LearningModels.MindMap>>
{
    public Task<LearningResult<LearningModels.MindMap>> Handle(GetMindMapQuery request, CancellationToken cancellationToken) => service.GetMindMapAsync(request.UserId, request.Word, request.HskLevel, request.MaxChildren, cancellationToken);
}
