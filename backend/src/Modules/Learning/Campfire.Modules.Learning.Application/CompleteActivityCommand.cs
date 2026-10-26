using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed record CompleteActivityCommand(Guid UserId, Guid ActivityId, int Score) : IRequest<LearningResult<LearningModels.Completion>>;
