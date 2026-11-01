using MediatR;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.GetMindMap;
public sealed record GetMindMapQuery(Guid UserId, string Word, int HskLevel, int MaxChildren) : IRequest<LearningResult<LearningModels.MindMap>>;
