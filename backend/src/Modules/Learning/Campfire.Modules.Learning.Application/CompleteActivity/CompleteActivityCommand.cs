using MediatR;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.CompleteActivity;
public sealed record CompleteActivityCommand(Guid UserId, Guid ActivityId, int Score) : IRequest<LearningResult<LearningModels.Completion>>;
