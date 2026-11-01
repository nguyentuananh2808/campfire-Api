using MediatR;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.GetDay;
public sealed record GetDayQuery(Guid UserId, Guid DayId) : IRequest<LearningResult<LearningModels.DayDetail>>;
