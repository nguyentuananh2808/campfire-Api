using MediatR;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.GetPath;
public sealed record GetPathQuery(Guid UserId, string Slug) : IRequest<LearningResult<LearningModels.CoursePath>>;
