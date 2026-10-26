using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed record GetMindMapQuery(Guid UserId, string Word, int HskLevel, int MaxChildren) : IRequest<LearningResult<LearningModels.MindMap>>;
