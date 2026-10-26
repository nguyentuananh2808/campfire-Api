using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed record GetDayQuery(Guid UserId, Guid DayId) : IRequest<LearningResult<LearningModels.DayDetail>>;
