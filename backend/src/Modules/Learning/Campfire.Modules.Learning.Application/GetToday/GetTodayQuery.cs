using MediatR;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.GetToday;
public sealed record GetTodayQuery(Guid UserId) : IRequest<LearningResult<LearningModels.Today>>;
