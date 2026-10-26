using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed record GetTodayQuery(Guid UserId) : IRequest<LearningResult<LearningModels.Today>>;
