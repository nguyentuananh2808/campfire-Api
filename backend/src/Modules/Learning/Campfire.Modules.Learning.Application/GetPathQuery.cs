using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed record GetPathQuery(Guid UserId, string Slug) : IRequest<LearningResult<LearningModels.CoursePath>>;
